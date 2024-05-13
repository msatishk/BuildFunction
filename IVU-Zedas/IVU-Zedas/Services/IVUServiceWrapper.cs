using IVUWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ToIVUMultipleFromOracle.Interfaces;
using ToIVUMultipleFromOracle.Models;
using Shared.Utils;
using System.Text.RegularExpressions;
using System.ServiceModel;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Index.HPRtree;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
namespace ToIVUMultipleFromOracle.Services
{
    public class IVUServiceWrapper //: IIVUServiceWrapper
    {
        private readonly IWCFClient<PersonnelImportPortTypeChannel> clientProxy;

        private readonly IVUPayloadSettings iVUPayloadSettings;
        string RemoveText(Match m) { return ""; }

        public IVUServiceWrapper(IVUPayloadSettings payloadSettings)
        {
            //clientProxy = new WCFClient<PersonnelImportPortTypeChannel>(payloadSettings);
            iVUPayloadSettings = payloadSettings;
        }

        public PersonnelImportPortTypeClient GetClient(IVUPayloadSettings iVUPayloadSettings)
        {
            var binding = new BasicHttpsBinding();
            binding.MaxBufferPoolSize = int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.Security.Mode = BasicHttpsSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            PersonnelImportPortTypeClient client = new PersonnelImportPortTypeClient(binding, new EndpointAddress(iVUPayloadSettings.IVUEndpoint));
            client.ClientCredentials.UserName.UserName = iVUPayloadSettings.Username;
            client.ClientCredentials.UserName.Password = iVUPayloadSettings.Password;
            iVUPayloadSettings.log.LogInformation($"ToIVUMultipleFromOracle: url: {iVUPayloadSettings.IVUEndpoint}, username:{iVUPayloadSettings.Username}, password: {iVUPayloadSettings.Password.Substring(iVUPayloadSettings.Password.Length-3)}");
            return client;
        }

        public IEnumerable<employeeResult> ImportStaffMemberships(string request)
        {
            request = Regex.Replace(request, @"<\w+?\/>", new MatchEvaluator(RemoveText));
            if (string.IsNullOrEmpty(request))
            {
                return new List<employeeResult>();
            }
            iVUPayloadSettings.log.LogInformation($"ImportStaffMemberships started");

            ImportStaffMembershipsRequest staffMem = Utils.DeserializeFromXmlString<ImportStaffMembershipsRequest>(request);
            staffMembership[] staffmem = staffMem.StaffMemberships.Select(ModelToDTO.ToStaffMemDto).ToArray();
            importStaffMemberships1 req = new importStaffMemberships1() { importStaffMembershipsRequest = new importStaffMemberships { staffMemberships = staffmem } };
            TimeSpan pauseBetweenFailures = Helper.GetPauseBetweenFailureTime(iVUPayloadSettings);
            IEnumerable<employeeResult> results = null;
            bool success = false;
            for (int i = 0; i < Convert.ToInt32(iVUPayloadSettings.MaxRetries); i++)
            {
                PersonnelImportPortTypeClient client = GetClient(iVUPayloadSettings);
                try
                {
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {pauseBetweenFailures.TotalSeconds}");
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {JsonConvert.SerializeObject(req.importStaffMembershipsRequest)}");

                    importEmployeesResult result = client.importStaffMemberships(req.importStaffMembershipsRequest);
                    if (result != null && result.employeeResults != null)
                    {
                        results = result.employeeResults.AsEnumerable();
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    iVUPayloadSettings.log.LogError(0, ex, $"ImportStaffMemberships method exception: Retry Attempt:{i + 1}");
                    if (i < Convert.ToInt32(iVUPayloadSettings.MaxRetries) - 1)
                    {
                        iVUPayloadSettings.log.LogInformation($"!!!!!!!!! Sleep for {pauseBetweenFailures.TotalSeconds}");
                        Thread.Sleep(pauseBetweenFailures);
                    }
                    else
                    {
                        iVUPayloadSettings.NetworkError = true;
                        throw;
                    }
                }
                finally
                {
                    if (client != null && client.State == CommunicationState.Opened)
                        client.Close();
                    else client?.Abort();
                }
                if (success)
                    break;
            }
            iVUPayloadSettings.log.LogInformation($"ImportStaffMemberships completed");
                return results;
        }

        public IEnumerable<employeeResult> ImportPersonnelData(string request)
        {
            //request = Regex.Replace(request, @"<\w+?/>", new MatchEvaluator(RemoveText));
            if (string.IsNullOrEmpty(request))
            {
                return new List<employeeResult>();
            }
            iVUPayloadSettings.log.LogInformation($"ImportPersonnelData Started");
            ImportPersonnelDataRequest staffRequest = Utils.DeserializeFromXmlString<ImportPersonnelDataRequest>(request);
            personnelData[] personelData = staffRequest.PersonnelData.Select(ModelToDTO.ToPersonnelData).ToArray();
            iVUPayloadSettings.PersonnelDataCount = personelData.Count();
           
            importPersonnelData req = new importPersonnelData() { importPersonnelDataRequest = personelData };
            TimeSpan pauseBetweenFailures = Helper.GetPauseBetweenFailureTime(iVUPayloadSettings);
            IEnumerable<employeeResult> results = null;
            bool success = false;
            for (int i = 0; i < Convert.ToInt32(iVUPayloadSettings.MaxRetries); i++)
            {
                PersonnelImportPortTypeClient client = GetClient(iVUPayloadSettings);
                try
                {
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {pauseBetweenFailures.TotalSeconds}");
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {JsonConvert.SerializeObject(req.importPersonnelDataRequest)}");

                    importEmployeesResult result = client.importPersonnelData(req.importPersonnelDataRequest);
                    if (result != null && result.employeeResults != null)
                    {
                        results = result.employeeResults.AsEnumerable();
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    iVUPayloadSettings.log.LogError(0, ex, $"ImportPersonnelData method exception: Retry Attempt:{i + 1}");
                    if (i < Convert.ToInt32(iVUPayloadSettings.MaxRetries) - 1)
                    {
                        iVUPayloadSettings.log.LogInformation($"!!!!!!!!! Sleep for {pauseBetweenFailures.TotalSeconds}");

                        Thread.Sleep(pauseBetweenFailures);
                    }
                    else
                    {
                        iVUPayloadSettings.NetworkError = true;
                        iVUPayloadSettings.log.LogInformation($"!!!!!!!!! channel abort!!!!!!");
                        throw;
                    }
                }
                finally
                {
                    if (client != null && client.State == CommunicationState.Opened)
                        client.Close();
                    else client?.Abort();
                }
                if (success)
                    break;
            }        
            iVUPayloadSettings.log.LogInformation($"ImportPersonnelData completed");
            return results;
            
        }

        //public IEnumerable<employeeResult> ImportDepotAssignments(string request)
        //{
        //    request = Regex.Replace(request, @"<\w+?/>", new MatchEvaluator(RemoveText));
        //    if (string.IsNullOrEmpty(request))
        //    {
        //        return new List<employeeResult>();
        //    }
        //    ImportDepotAssignmentsRequest depotRequest = Utils.DeserializeFromXmlString<ImportDepotAssignmentsRequest>(request);
        //    depotAssignments[] depotAssignments = depotRequest.DepotAssignments.Select(ModelToDTO.ToDepotAssignmentsDto).ToArray();
        //    importDepotAssignments1 req = new importDepotAssignments1()
        //    {
        //        importDepotAssignmentsRequest = new importDepotAssignments()
        //        {
        //            depotAssignments = depotAssignments
        //        }
        //    };
        //    TimeSpan pauseBetweenFailures = GetPauseBetweenFailureTime(iVUPayloadSettings);
        //    try
        //    {
        //        importDepotAssignmentsResponse result = Utils.Execute(
        //            () =>
        //            {
        //                return clientProxy.Execute(client => client.importDepotAssignments(req));
        //            },
        //            iVUPayloadSettings.log, Convert.ToInt32(iVUPayloadSettings.MaxRetries),
        //            pauseBetweenFailures);
        //        return result.importEmployeesResult.employeeResults.AsEnumerable();
        //    }
        //    catch (Exception ex)
        //    {
        //        iVUPayloadSettings.NetworkError = true;
        //        throw;
        //    }
        //}

        //public IEnumerable<employeeResult> ImportCostCenterAssignments(string request)
        //{
        //    request = Regex.Replace(request, @"<\w+?/>", new MatchEvaluator(RemoveText));
        //    if (string.IsNullOrEmpty(request))
        //    {
        //        return new List<employeeResult>();
        //    }
        //    ImportCostCenterAssignmentsRequest costCenterRequest = Utils.DeserializeFromXmlString<ImportCostCenterAssignmentsRequest>(request);
        //    costCenterAssignments[] costCenterAssignments = costCenterRequest.CostCenterAssignments.Select(ModelToDTO.ToCostCenterAssignmentsDto).ToArray();
        //    importCostCenterAssignments req = new importCostCenterAssignments() { importCostCenterAssignmentsRequest = costCenterAssignments };
        //    TimeSpan pauseBetweenFailures = GetPauseBetweenFailureTime(iVUPayloadSettings);
        //    try
        //    {
        //        importCostCenterAssignmentsResponse result = Utils.Execute(
        //            () => { return clientProxy.Execute(client => client.importCostCenterAssignments(req)); },
        //            iVUPayloadSettings.log, Convert.ToInt32(iVUPayloadSettings.MaxRetries),
        //            pauseBetweenFailures);
        //        return result.importEmployeesResult.employeeResults.AsEnumerable();
        //    }
        //    catch (Exception ex) 
        //    {
        //        iVUPayloadSettings.NetworkError = true;
        //        throw; 
        //    }
        //}

        public IEnumerable<employeeResult> ImportQualificationAssignmentsRequest(string request)
        {
            //request = Regex.Replace(request, @"<\w+?/>", new MatchEvaluator(RemoveText));
            if (string.IsNullOrEmpty(request))
            {
                return new List<employeeResult>();
            }
            iVUPayloadSettings.log.LogInformation($"ImportQualificationAssignmentsRequest Started");

            ImportQualificationAssignmentsRequest qualRequest = Utils.DeserializeFromXmlString<ImportQualificationAssignmentsRequest>(request);
            qualificationAssignments[] qualAssignments = qualRequest.QualificationAssignments.Select(ModelToDTO.ToQualAssignmentsDto).ToArray();
            importQualificationAssignments1 req = new importQualificationAssignments1()
            {

                importQualificationAssignmentsRequest = new importQualificationAssignments()
                {
                    qualificationAssignments = qualAssignments
                }
            };
            TimeSpan pauseBetweenFailures = Helper.GetPauseBetweenFailureTime(iVUPayloadSettings);
            IEnumerable<employeeResult> results = null;
            bool success = false;
            for (int i = 0; i < Convert.ToInt32(iVUPayloadSettings.MaxRetries); i++)
            {
                PersonnelImportPortTypeClient client = GetClient(iVUPayloadSettings);
                try
                {
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {pauseBetweenFailures.TotalSeconds}");
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {JsonConvert.SerializeObject(req.importQualificationAssignmentsRequest)}");

                    importEmployeesResult result = client.importQualificationAssignments(req.importQualificationAssignmentsRequest);
                    if (result != null && result.employeeResults != null)
                    {
                        results = result.employeeResults.AsEnumerable();
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    iVUPayloadSettings.log.LogError(0, ex, $"ImportQualificationAssignmentsRequest method exception: Retry Attempt:{i + 1}");
                    if (i < Convert.ToInt32(iVUPayloadSettings.MaxRetries) - 1)
                    {
                        iVUPayloadSettings.log.LogInformation($"!!!!!!!!! Sleep for {pauseBetweenFailures.TotalSeconds}");
                        Thread.Sleep(pauseBetweenFailures);
                    }
                    else
                    {
                        iVUPayloadSettings.NetworkError = true;
                        throw;
                    }
                }
                finally
                {
                    if (client != null && client.State == CommunicationState.Opened)
                        client.Close();
                    else client?.Abort();
                }
                if (success)
                    break;
            }
            iVUPayloadSettings.log.LogInformation($"ImportQualificationAssignmentsRequest completed");
            return results;
        }

        public IEnumerable<employeeResult> ImportAttributeAssignmentsRequest(string request)
        {
           // request = Regex.Replace(request, @"<\w+?\/>", new MatchEvaluator(RemoveText));
            if (string.IsNullOrEmpty(request))
            {
                return new List<employeeResult>();
            }
            iVUPayloadSettings.log.LogInformation($"ImportAttributeAssignmentsRequest started");

            ImportAttributeAssignmentsRequest attributeRequest = Utils.DeserializeFromXmlString<ImportAttributeAssignmentsRequest>(request);
            attributeAssignments[] attributeAssignments = attributeRequest.AttributeAssignments.Select(ModelToDTO.ToAttributeAssignmentsDto).ToArray();
            importAttributeAssignments req = new importAttributeAssignments() { importAttributeAssignmentsRequest = attributeAssignments };
            TimeSpan pauseBetweenFailures = Helper.GetPauseBetweenFailureTime(iVUPayloadSettings);
            IEnumerable<employeeResult> results = null;
            bool success = false;
            for (int i = 0; i < Convert.ToInt32(iVUPayloadSettings.MaxRetries); i++)
            {
                PersonnelImportPortTypeClient client = GetClient(iVUPayloadSettings);
                try
                {
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {pauseBetweenFailures.TotalSeconds}");
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {JsonConvert.SerializeObject(req.importAttributeAssignmentsRequest)}");

                    importEmployeesResult result = client.importAttributeAssignments(req.importAttributeAssignmentsRequest);
                    if (result != null && result.employeeResults != null)
                    {
                        results = result.employeeResults.AsEnumerable();
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    iVUPayloadSettings.log.LogError(0, ex, $"importAttributeAssignmentsRequest method exception: Retry Attempt:{i + 1}");
                    if (i < Convert.ToInt32(iVUPayloadSettings.MaxRetries) - 1)
                    {
                        iVUPayloadSettings.log.LogInformation($"!!!!!!!!! Sleep for {pauseBetweenFailures.TotalSeconds}");
                        Thread.Sleep(pauseBetweenFailures);
                    }
                    else
                    {
                        iVUPayloadSettings.NetworkError = true;
                        throw;
                    }
                }
                finally
                {
                    if (client != null && client.State == CommunicationState.Opened)
                        client.Close();
                    else client?.Abort();
                }
                if (success)
                    break;
            }
            iVUPayloadSettings.log.LogInformation($"ImportAttributeAssignmentsRequest completed");
            return results;
        }

        public IEnumerable<employeeResult> ImportEmployeeGroupAssignments(string request)
        {
            //request = Regex.Replace(request, @"<\w+?/>", new MatchEvaluator(RemoveText));
            if (string.IsNullOrEmpty(request))
            {
                return new List<employeeResult>();
            }
            iVUPayloadSettings.log.LogInformation($"ImportEmployeeGroupAssignments started");

            ImportEmployeeGroupAssignmentsRequest empGroupAssignRequest = Utils.DeserializeFromXmlString<ImportEmployeeGroupAssignmentsRequest>(request);
            employeeGroupAssignments[] employeeAssignments = empGroupAssignRequest.EmployeeGroupAssignments.Select(ModelToDTO.ToEmployeeGroupAssignmentsDto).ToArray();
            importEmployeeGroupAssignments1 req = new importEmployeeGroupAssignments1()
            {
                importEmployeeGroupAssignmentsRequest = new importEmployeeGroupAssignments()
                {
                    employeeGroupAssignments = employeeAssignments
                }
            };
            TimeSpan pauseBetweenFailures = Helper.GetPauseBetweenFailureTime(iVUPayloadSettings);
            IEnumerable<employeeResult> results = null;
            bool success = false;
            for (int i = 0; i < Convert.ToInt32(iVUPayloadSettings.MaxRetries); i++)
            {
                PersonnelImportPortTypeClient client = GetClient(iVUPayloadSettings);
                try
                {
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {pauseBetweenFailures.TotalSeconds}");
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {JsonConvert.SerializeObject(req.importEmployeeGroupAssignmentsRequest)}");

                    importEmployeesResult result = client.importEmployeeGroupAssignments(req.importEmployeeGroupAssignmentsRequest);
                    if (result != null && result.employeeResults != null)
                    {
                        results = result.employeeResults.AsEnumerable();
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    iVUPayloadSettings.log.LogError(0, ex, $"ImportEmployeeGroupAssignmentsRequest method exception: Retry Attempt:{i + 1}");
                    if (i < Convert.ToInt32(iVUPayloadSettings.MaxRetries) - 1)
                    {
                        iVUPayloadSettings.log.LogInformation($"!!!!!!!!! Sleep for {pauseBetweenFailures.TotalSeconds}");
                        Thread.Sleep(pauseBetweenFailures);
                    }
                    else
                    {
                        iVUPayloadSettings.NetworkError = true;
                        throw;
                    }
                }
                finally
                {
                    if (client != null && client.State == CommunicationState.Opened)
                        client.Close();
                    else client?.Abort();
                }
                if (success)
                    break;
            }
            iVUPayloadSettings.log.LogInformation($"ImportEmployeeGroupAssignments completed");
            return results;
        }

        public IEnumerable<employeeResult> ImportAbsenceAndAttendanceAllocationsRequest(string request)
        {
            //request = Regex.Replace(request, @"<\w+?/>", new MatchEvaluator(RemoveText));
            if (string.IsNullOrEmpty(request))
            {
                return new List<employeeResult>();
            }
            iVUPayloadSettings.log.LogInformation($"ImportAbsenceAndAttendanceAllocationsRequest started");

            ImportAbsenceAndAttendanceAllocationsRequest absenceAttendenceRequest = Utils.DeserializeFromXmlString<ImportAbsenceAndAttendanceAllocationsRequest>(request);
            absenceAndAttendanceAllocations[] absenceAllocations = absenceAttendenceRequest.AbsenceAndAttendanceAllocations.Select(ModelToDTO.ToAbsenceAndAttendanceAllocationsDto).ToArray();
            importAbsenceAndAttendanceAllocations1 req = new importAbsenceAndAttendanceAllocations1()
            {
                importAbsenceAndAttendanceAllocationsRequest = new importAbsenceAndAttendanceAllocations()
                {
                    absenceAndAttendanceAllocations = absenceAllocations
                }
            };
            TimeSpan pauseBetweenFailures = Helper.GetPauseBetweenFailureTime(iVUPayloadSettings);
            IEnumerable<employeeResult> results = null;
            bool success = false;
            for (int i = 0; i < Convert.ToInt32(iVUPayloadSettings.MaxRetries); i++)
            {
                PersonnelImportPortTypeClient client = GetClient(iVUPayloadSettings);
                try
                {
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {pauseBetweenFailures.TotalSeconds}");
                    iVUPayloadSettings.log.LogInformation($"!!!!!!!!! {JsonConvert.SerializeObject(req.importAbsenceAndAttendanceAllocationsRequest)}");

                    importEmployeesResult result = client.importAbsenceAndAttendanceAllocations(req.importAbsenceAndAttendanceAllocationsRequest);
                    if (result != null && result.employeeResults != null)
                    {
                        results = result.employeeResults.AsEnumerable();
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    iVUPayloadSettings.log.LogError(0, ex, $"ImportAbsenceAndAttendanceAllocationsRequest method exception: Retry Attempt:{i + 1}");
                    if (i < Convert.ToInt32(iVUPayloadSettings.MaxRetries) - 1)
                    {
                        iVUPayloadSettings.log.LogInformation($"!!!!!!!!! Sleep for {pauseBetweenFailures.TotalSeconds}");
                        Thread.Sleep(pauseBetweenFailures);
                    }
                    else
                    {
                        iVUPayloadSettings.NetworkError = true;
                        throw;
                    }
                }
                finally
                {
                    if (client != null && client.State == CommunicationState.Opened)
                        client.Close();
                    else client?.Abort();
                }
                if (success)
                    break;
            }
            iVUPayloadSettings.log.LogInformation($"ImportAbsenceAndAttendanceAllocationsRequest completed");
            return results;
        }
    }
}
