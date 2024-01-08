using System;

namespace SnitzCore.Service
{
    public class PostTopicCreate
    {
        public event EventHandler<EventArgs> TopicCreated;

        protected virtual void OnTopicCreated(EventArgs e)
        {
            EventHandler<EventArgs> handler = TopicCreated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

    }

}
