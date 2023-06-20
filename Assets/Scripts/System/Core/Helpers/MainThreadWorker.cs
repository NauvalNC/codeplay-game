using System;
using System.Collections.Generic;
using UnityEngine;

internal class MainThreadWorker : MonoBehaviour
{
    internal static MainThreadWorker worker;
    Queue<Action> jobs = new Queue<Action>();

    void Awake()
    {
        if (!worker) worker = this;
    }

    void Update()
    {
        while (jobs.Count > 0)
        {
            jobs.Dequeue().Invoke();
        }
    }

    internal void AddJob(Action newJob)
    {
        jobs.Enqueue(newJob);
    }
}