using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPoolProject
{
    public interface IErrorMessageHelper
    {
        void SentMessage(Exception exception, string? nameThread);
    }
}
