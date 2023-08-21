using System;
namespace PEAS.Models.Maps
{
    public struct LocationResponse
    {
        public struct AddressRoot
        {
            public AddressExpanded Address { get; set; }
            public string Position { get; set; }
            public string Id { get; set; }
        }

        public struct AddressExpanded
        {
            public string CountryCode { get; set; }
            public string? CountrySubdivision { get; set; }
            public string? Municipality { get; set; }
            public string? Country { get; set; }
            public string? CountrySubdivisionName { get; set; }
            public string? LocalName { get; set; }
        }

        public struct SummaryRoot
        {
            public int QueryTime { get; set; }
            public int NumResults { get; set; }
        }

        public SummaryRoot Summary { get; set; }
        public List<AddressRoot> Addresses { get; set; }
    }

    public struct TimeZoneResponse
    {
        public struct TimeZoneExpanded
        {
            public string Id { get; set; }
        }

        public string Version { get; set; }
        public List<TimeZoneExpanded> TimeZones { get; set; }
    }
}