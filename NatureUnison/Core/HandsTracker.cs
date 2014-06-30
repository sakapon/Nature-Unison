using System;
using System.Collections.Generic;
using System.Linq;

namespace NatureUnison
{
    public abstract class HandsTracker
    {
        internal HandsContext Context { get; set; }

        protected void NotifySingleHandFrame(HandFrame? f)
        {
            var context = Context;
            if (context != null)
            {
                context.NotifySingleHandFrame(f);
            }
        }
    }
}
