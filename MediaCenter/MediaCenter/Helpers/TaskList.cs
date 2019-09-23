//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MediaCenter.Helpers
//{
//    public class TaskList
//    {
//        public enum CloseBehavior { Wait, Abort}

//        private struct TaskListEntry
//        {
//            public Task Task;
//            public CloseBehavior Behavior;

//            public TaskListEntry(Task task, CloseBehavior behavior)
//            {
//                Task = task;
//                Behavior = behavior;
//            }
//        }

//        private List<TaskListEntry> _taskList;

//        public TaskList()
//        {
//            _taskList = new List<TaskListEntry>();
//        }

//        public void AddTask(Task task, CloseBehavior closeBehavior)
//        {
//            var entry = new TaskListEntry(task, closeBehavior);
//            task.ContinueWith((t) => _taskList.Remove(entry));
//        }

//        public void Close()
//        {
//            foreach (var entry in _taskList.ToList())
//            {
//                switch (entry.Behavior)
//                {
//                    case CloseBehavior.Abort:
//                        entry.Task.
//                }
//            }
//        }
//    }
//}
