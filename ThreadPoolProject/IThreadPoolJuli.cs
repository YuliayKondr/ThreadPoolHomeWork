using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPoolProject
{
    public interface IThreadPoolJuli
    {
        void Execute(Action<object?> action, object? value, PriorityThread priority = PriorityThread.NORMAL);

        void Stop();
    }
}
