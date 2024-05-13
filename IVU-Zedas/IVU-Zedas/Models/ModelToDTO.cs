using IVUWS;
using Microsoft.Azure.Amqp.Framing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToIVUMultipleFromOracle.Models
{
    public class ModelToDTO
    {
        public static staffMembership ToStaffMemDto(StaffMemberships staffMembership)
        {
            return new staffMembership
            {
                personnelNumber = staffMembership.PersonnelNumber,
                hireDate = staffMembership.HireDate
            };
        }

        public static personnelData ToPersonnelData(PersonnelData personnelData)
        {
            return new personnelData
            {
                personnelNumber = personnelData.PersonnelNumber,
                firstName = personnelData.FirstName,
                lastName = personnelData.LastName,
                dateOfBirth = personnelData.DateOfBirth,
                gender = personnelData.Gender,
                address = personnelData.Address.Select(x => new address() { co = x.Co, country = x.Country, street = x.Street, district = x.District, postalCode = x.PostalCode, town = x.Town, name = x.Name }).ToArray(),
                telephoneNumber = personnelData.TelephoneNumber.Select(x => new telephoneNumber() { name = x.Name, number = x.Number }).ToArray(),
                emailAddress = personnelData.EmailAddress.Select(x => new emailAddress() { name = x.Name, address = x.Address }).ToArray(),
                cardNumber = personnelData.CardNumber,
                cardNumberSpecified = personnelData.CardNumberSpecified,
                title = personnelData.Title,
                formOfAddress = personnelData.FormOfAddress,
                ignoreExistingComment = personnelData.IgnoreExistingComment,
            };
        }

        //public static depotAssignments ToDepotAssignmentsDto(DepotAssignments depotAssignments)
        //{
        //    var depotAssign = new depotAssignments
        //    {
        //        personnelNumber = depotAssignments.PersonnelNumber,
        //        importDateRange = new dateRange()
        //        {
        //            endDate = depotAssignments.ImportDateRangeDepot.EndDate,
        //            endDateSpecified = depotAssignments.ImportDateRangeDepot.EndDateSpecified,
        //            startDate = depotAssignments.ImportDateRangeDepot.StartDate,
        //            startDateSpecified = depotAssignments.ImportDateRangeDepot.StartDateSpecified
        //        },
        //    };
        //    depotAssign.depotAssignment = depotAssignments.DepotAssignment.Select(depot => new depotAssignment() { dateRange = new dateRange() { endDateSpecified = depot.EndDateSpecified, endDate = depot.DateRange.EndDate, startDate = depot.DateRange.StartDate, startDateSpecified = depot.StartDateSpecified }, abbreviation = depot.Abbreviation, externalNumber = depot.ExternalNumber }).ToArray();
        //    return depotAssign;
        //}

        //public static costCenterAssignments ToCostCenterAssignmentsDto(CostCenterAssignments costCenterAssignments)
        //{
        //    var costAssign = new costCenterAssignments()
        //    {
        //        personnelNumber = costCenterAssignments.PersonnelNumber,
        //        importDateRange = new dateRange()
        //        {
        //            endDate = costCenterAssignments.ImportDateRange.EndDate,
        //            endDateSpecified = costCenterAssignments.ImportDateRange.EndDateSpecified,
        //            startDate = costCenterAssignments.ImportDateRange.StartDate,
        //            startDateSpecified = costCenterAssignments.ImportDateRange.StartDateSpecified
        //        },
        //    };
        //    costAssign.costCenterAssignment = costCenterAssignments.CostCenterAssignment.Select(costCenter => new costCenterAssignment() { dateRange = new dateRange()
        //    {
        //        endDateSpecified = costCenter.DateRange.EndDateSpecified,
        //        endDate = costCenter.DateRange.EndDate, startDate = costCenter.DateRange.StartDate, 
        //        startDateSpecified = costCenter.DateRange.StartDateSpecified
        //    }, abbreviation = costCenter.Abbreviation, }).ToArray();
        //    return costAssign;
        //}

        public static qualificationAssignments ToQualAssignmentsDto(QualificationAssignments qualAssignments)
        {
            var qualAssign = new qualificationAssignments { personnelNumber = qualAssignments.PersonnelNumber };
            qualAssign.qualificationAssignment = qualAssignments.QualificationAssignment.Select(qual =>
                new qualificationAssignment()
                {
                    assignmentDateRange = GetAssignmentDateRange(qual),
                    externalNumber = qual.ExternalNumber,
                    importDateRange = new dateRange()
                    {
                        endDate = qual.ImportDateRange.EndDate,
                        endDateSpecified = qual.ImportDateRange.EndDateSpecified,
                        startDate = qual.ImportDateRange.StartDate,
                        startDateSpecified = qual.ImportDateRange.StartDateSpecified
                    },
                    qualification = qual.Qualification,
                    qualificationClass = qual.QualificationClass,
                    qualificationGroup = qual.QualificationGroup
                }).ToArray();
            return qualAssign;
        }

        private static dateRange[] GetAssignmentDateRange(QualificationAssignment qual)
        {
            List<dateRange> dateRanges = new List<dateRange>();
            foreach (AssignmentDateRange assigndateRange in qual.AssignmentDateRange)
            {
                dateRange date = new dateRange();
                if (assigndateRange.EndDate != default)
                    date.endDate = assigndateRange.EndDate;

                date.endDateSpecified = assigndateRange.EndDateSpecified;
                date.startDate = assigndateRange.StartDate;
                date.startDateSpecified = assigndateRange.StartDateSpecified;
                dateRanges.Add(date);
            }
            return dateRanges.ToArray();
        }

        public static attributeAssignments ToAttributeAssignmentsDto(AttributeAssignments attributeAssignments)
        {
            var attrAssign = new attributeAssignments() { personnelNumber = attributeAssignments.PersonnelNumber };
            attrAssign.attributeAssignment = attributeAssignments.AttributeAssignment.Select(attr => new attributeAssignment()
            {
                attributeName = attr.AttributeName,
                    attributeValue = attr.AttributeValue
            }).ToArray();
            return attrAssign;
        }

        public static employeeGroupAssignments ToEmployeeGroupAssignmentsDto(EmployeeGroupAssignments employeeGroupAssignments)
        {
            var empGroupAssign = new employeeGroupAssignments() { personnelNumber = employeeGroupAssignments.PersonnelNumber };
            empGroupAssign.employeeGroupAssignment = employeeGroupAssignments.EmployeeGroupAssignment.Select(qual =>
                new employeeGroupAssignment()
                {
                        assignmentDateRange = qual.EmpAssignmentDateRange.Select(x => new dateRange()
                        {
                            endDate = x.EndDate,
                            startDate = x.StartDate
                        }).ToArray(),
                    group= qual.Group,
                    groupType= qual.GroupType,
                    importDateRange = new dateRange()
                    {
                        endDate = qual.EmpImportDateRange.EndDate,
                        startDate = qual.EmpImportDateRange.StartDate
                    }
                }).ToArray();          
            return empGroupAssign;
        }

        public static absenceAndAttendanceAllocations ToAbsenceAndAttendanceAllocationsDto(AbsenceAndAttendanceAllocations absenceAndAttendanceAllocations)
        {
            var absenceAttendanceAllocation = new absenceAndAttendanceAllocations() { personnelNumber = absenceAndAttendanceAllocations.PersonnelNumber };
            absenceAttendanceAllocation.allocation = absenceAndAttendanceAllocations.Allocation.Select(abs =>
                new absenceOrAttendanceAllocation()
                {
                    importTimeRange = new timeRange()
                    {
                        endTime = abs.ImportTimeRange.EndTime,
                        startTime = abs.ImportTimeRange.StartTime
                    },
                    abbreviation = abs.Abbreviation,
                    comment = abs.Comment,
                    planningLevel = abs.PlanningLevel,
                    type = abs.Type,
                    strategy = abs.Strategy,
                    paidTime = abs.PaidTime,
                    workTime = abs.WorkTime,
                    allocationTimeRange = abs.AllocationTimeRange.Select(x => new boundedTimeRange()
                    {
                        startTime = x.StartTime ,
                        endTime = x.EndTime,
                    }).ToArray(),

                }).ToArray();
            return absenceAttendanceAllocation;
        }
    }
}
