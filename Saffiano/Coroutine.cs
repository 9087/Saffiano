using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saffiano
{
    public abstract class YieldInstruction
    {
        internal List<YieldInstruction> parents = new List<YieldInstruction>();
        internal bool finished = false;
        internal bool interrupted = false;

        internal abstract void Start();

        internal void Finish()
        {
            if (this.finished)
            {
                throw new Exception();
            }
            this.finished = true;
            foreach (YieldInstruction parent in this.parents)
            {
                parent?.Start();
            }
            this.parents.Clear();
        }

        internal virtual void Interrupt()
        {
            this.interrupted = true;
            this.parents.Clear();
        }

        internal void PushParent(YieldInstruction parent)
        {
            if (this.interrupted)
            {
                return;
            }
            else if (this.finished)
            {
                parent?.Start();
            }
            else
            {
                this.parents.Add(parent);
            }
        }
    }

    public class Coroutine : YieldInstruction
    {
        private IEnumerator routine = null;
        private YieldInstruction child = null;

        public Coroutine(IEnumerator routine)
        {
            this.routine = routine;
        }

        internal override void Start()
        {
            while (this.routine.MoveNext())
            {
                object current = this.routine.Current;
                if (this.child != null && !this.child.finished)
                {
                    throw new Exception();
                }
                if (current is null)
                {
                    this.child = new WaitForSeconds(0);
                }
                else
                {
                    Type currentType = current.GetType();
                    if (currentType.IsSubclassOf(typeof(YieldInstruction)))
                    {
                        this.child = (YieldInstruction)current;
                    }
                    else if (current is IEnumerator)
                    {
                        this.child = new Coroutine((IEnumerator)current);
                    }
                    else
                    {
                        continue;
                    }
                }
                this.child.PushParent(this);
                this.child.Start();
                if (!this.child.finished)
                {
                    return;
                }
            }
            this.Finish();
        }

        internal override void Interrupt()
        {
            base.Interrupt();
            this.child.Interrupt();
        }
    }

    public class WaitForSeconds : YieldInstruction
    {
        private float seconds = 0;
        private Timer timer = null;

        public WaitForSeconds(float seconds)
        {
            this.seconds = seconds;
        }

        internal override void Start()
        {
            this.timer = Timer.Create(this.seconds, delegate ()
            {
                this.Finish();
            });
        }

        internal override void Interrupt()
        {
            base.Interrupt();
            Timer.Destroy(this.timer);
        }
    }

    public abstract class AsyncOperation : YieldInstruction
    {
        public AsyncOperation()
        {
        }

        public bool isDone
        {
            get
            {
                return this.finished;
            }
        }

        public float progress
        {
            get;
            protected set;
        }

        internal abstract void Wait();
    }

    public sealed class ResourceRequest : AsyncOperation
    {
        private String path = null;
        private Task<Object> task = null;
        System.Threading.CancellationTokenSource cts = null;

        public ResourceRequest(String path)
        {
            this.path = path;
        }

        public Object asset
        {
            get;
            private set;
        }

        internal void OnProgressChanged(float progress)
        {
            this.progress = progress;
        }

        internal override void Start()
        {
            this.cts = new System.Threading.CancellationTokenSource();
            this.task = new Task<Object>(() => { var asset = Resources.LoadInternal(this.path); OnProgressChanged(1.0f); return asset; }, this.cts.Token);
            this.task.ContinueWith((Task<Object> task) =>
            {
                this.asset = task.Result;
                this.Finish();
            });
            this.task.Start();
        }

        internal override void Wait()
        {
            if (this.finished)
            {
                return;
            }
            if (this.task is null)
            {
                this.Start();
            }
            this.task.Wait();
        }

        internal override void Interrupt()
        {
            base.Interrupt();
            this.cts.Cancel();
        }
    }
}