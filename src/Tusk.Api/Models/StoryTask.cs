namespace Tusk.Api.Models
{
    // Dummy to show 1:n relation
    public class StoryTask : EntityBase
    {
        protected StoryTask()
        {
        }

        public StoryTask(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }
}
