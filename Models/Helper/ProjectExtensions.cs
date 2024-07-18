namespace BauphysikToolWPF.Models.Helper
{
    public static class ProjectExtensions
    {
        public static void AssignInternalIdsToElements(this Project project)
        {
            int index = 0; // Start at 0
            project.Elements.ForEach(e => e.InternalId = index++);
        }
    }
}
