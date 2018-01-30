namespace ValidationFramework.BusinessEntities
{
    public class FileResult
    {
        public string FileName { get; set; }

        public string FilePath { get; set; }
        public string TypeOfError { get; set; }
        public int NumberOfValidRows { get; set; }

        public int NumberOfErrorRows { get; set; }

        public bool CanBeReprocessed { get; set; }
    }
}
