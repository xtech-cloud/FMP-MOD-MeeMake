using MeeX.MeeMake;

namespace MeeX.XMA
{

    public class StoryProxy : IStoryProxy
    {
        public Runtime runtime {get;set;}
        
        public void JumpStory(string _storyName)
        {
            runtime.RenderStory(_storyName);
        }
    }//class
}//namespace
