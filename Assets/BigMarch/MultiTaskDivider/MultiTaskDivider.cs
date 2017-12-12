using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine;

public class MultiTaskDivider
{
	private class Task
	{
		// 这个任务的唯一标识，用于过滤重复的任务。
		public string TaskId;
		// 任务的具体逻辑
		public Action TaskAction;
	}

	// dic 的作用，紧紧是为了过滤重复任务。
	private Dictionary<string, Task> _taskDic = new Dictionary<string, Task>();

	// 任务队列
	private Queue<Task> _queue = new Queue<Task>();

	public void AddTaskIfNotExist(string taskId, Action taskAction)
	{
		// 如果是个新的 task，那么就加到 dic 和 list 中。
		if (!_taskDic.ContainsKey(taskId))
		{
			Task newTask = new Task();
			newTask.TaskId = taskId;
			newTask.TaskAction = taskAction;

			_taskDic.Add(taskId, newTask);

			_queue.Enqueue(newTask);

//			Debug.Log("MultiTaskDivider : add new task " + taskId);
		}
	}

	public void ExecuteOne()
	{
		if (_queue.Count == 0)
		{
			return;
		}

		// 拿出来一个任务，执行，然后放到队尾，继续排队。
		Task taskGoingToDo = _queue.Dequeue();
		taskGoingToDo.TaskAction();
		_queue.Enqueue(taskGoingToDo);
	}
}

