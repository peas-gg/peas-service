namespace PEAS
{
    public struct CalenderLinksResponse
    {
        public struct DataType
        {
            public string Id { get; set; }
            public string Secret { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Location { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public bool All_day { get; set; }
        }

        public struct LinksType
        {
            public string Event_page { get; set; }
            public string Apple { get; set; }
            public string Google { get; set; }
            public string Office365 { get; set; }
            public string Outlook { get; set; }
            public string Outlookcom { get; set; }
            public string Yahoo { get; set; }
            public string Stream { get; set; }
        }
        public DataType Data { get; set; }
        public LinksType Links { get; set; }
    }
}