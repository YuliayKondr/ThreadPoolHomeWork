using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreadPoolProject;

namespace ThreadPoolHomeWork
{
    internal sealed class MessageErrorShower : IErrorMessageHelper
    {
        void IErrorMessageHelper.SentMessage(Exception exception, string? nameThread)
        {
            Console.WriteLine("{0} - {1}",nameThread ?? string.Empty, exception.ToString());
        }
    }
}
