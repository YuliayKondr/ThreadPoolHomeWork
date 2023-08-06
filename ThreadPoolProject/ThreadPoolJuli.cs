using System.Collections.Concurrent;
using System.Reflection.Metadata;

namespace ThreadPoolProject
{
    public sealed class ThreadPoolJuli : IThreadPoolJuli
    {
        private const int MAX_ACTIVE_WORK = 3;

        private object _lock = new object();       

        private ConcurrentQueue<(Action<object?> WorkAction, object? Parameter)> _lowWorkAtions;
        private ConcurrentQueue<(Action<object?> WorkAction, object? Parameter)> _highWorkAtions;
        private ConcurrentQueue<(Action<object?> WorkAction, object? Parameter)> _normalWorkAtions;
        private readonly Thread[] _threads;
        private readonly IErrorMessageHelper? _errorMessageHelper;
        private volatile bool _isStoped;
        private volatile short _countActivateHighWork;
        
        private int _maxThreads;

        public ThreadPoolJuli(int maxThreadCount)
        {
            if(maxThreadCount <= 0)
            {
                throw new ArgumentOutOfRangeException("Максимальна кількість потоків повинна бути більше нуля");
            }

            _maxThreads = maxThreadCount;
            _threads = new Thread[_maxThreads];
                        
            Initialize();
        }

        public ThreadPoolJuli(int maxThreadCount, IErrorMessageHelper errorMessageHelper) : this(maxThreadCount)
        {
            _errorMessageHelper = errorMessageHelper;
        }

        public bool Execute(Action<object?> action, object? value, PriorityThread priority)
        {
            if (_isStoped)
            {
                return false;
            }

            switch (priority)
            {
                case PriorityThread.NORMAL:                    
                    _normalWorkAtions.Enqueue(new(action, value)); break;
                case PriorityThread.HIGH:                    
                    _highWorkAtions.Enqueue(new(action, value)); break;
                case PriorityThread.LOW:                    
                    _lowWorkAtions.Enqueue(new(action, value)); 
                    break;
            } 
            
            return true;
        }

        public void Stop()
        {
            _isStoped = true;
        }

        private void Initialize()
        {   
            _countActivateHighWork = 0;
            _normalWorkAtions = new ConcurrentQueue<(Action<object?> WorkAction, object? Parameter)>();
            _highWorkAtions = new ConcurrentQueue<(Action<object?> WorkAction, object? Parameter)>();
            _lowWorkAtions = new ConcurrentQueue<(Action<object?> WorkAction, object? Parameter)>();

            for (int i = 0; i < _threads.Length; i++)
            {
                string nameThread = $"{nameof(ThreadPoolJuli)}_{i}";

                _threads[i] = new Thread(Work)
                {
                    IsBackground = true,
                    Name = nameThread
                };

                _threads[i].Start();
            }
        }

        private void Work()
        {
            while (true)
            {
                (Action<object?> Work, object? Parameter) actionWork = default;

                lock (_lock)
                {
                    if(_highWorkAtions.IsEmpty && _normalWorkAtions.IsEmpty)
                    {
                        if(_lowWorkAtions.TryDequeue(out (Action<object?> Work, object ? Parameter) lowActionWork))
                        {
                            actionWork = lowActionWork;                            
                        }                       
                    }

                    if(!_highWorkAtions.IsEmpty && _countActivateHighWork < MAX_ACTIVE_WORK && _highWorkAtions.TryDequeue(out (Action<object?> Work, object? Parameter) highActionWork))
                    {
                        ++_countActivateHighWork;

                        actionWork = highActionWork;

                        continue;
                    }

                    _countActivateHighWork = 0;

                    if(_normalWorkAtions.TryDequeue(out (Action<object?> Work, object? Parameter) normalActionWork))
                    {
                        actionWork = normalActionWork;
                    }                   
                };

                WorkAction(actionWork);
            }
        }

        private void WorkAction((Action<object?> Work, object? Parameter) actionWork) 
        {
            if(actionWork == default)
            {
                return;
            }

            try
            {
                actionWork.Work?.Invoke(actionWork.Parameter);
            }
            catch (Exception ex)
            {
                _errorMessageHelper?.SentMessage(ex, Thread.CurrentThread.Name);
            }        
        }     
    }
}