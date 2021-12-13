﻿namespace FileFlows.Node.Workers
{
    using FileFlows.Node.FlowExecution;
    using FileFlows.ServerShared.Services;
    using FileFlows.ServerShared.Workers;
    using FileFlows.Shared;
    using FileFlows.Shared.Models;

    public class FlowWorker : Worker
    {
        public readonly Guid Uid = Guid.NewGuid();

        private readonly List<FlowRunner> ExecutingRunners = new List<FlowRunner>();

        private readonly bool isServer;

        public FlowWorker(bool isServer = false) : base(ScheduleType.Second, 5)
        {
            this.isServer = isServer;
        }

        protected override void Execute()
        {
            Logger.Instance?.DLog("FlowWorker.Execute");
            var nodeService = NodeService.Load();
            var node = isServer ? nodeService.GetServerNode().Result : nodeService.Register(Environment.MachineName).Result;
            node.Enabled = true;
            if (string.IsNullOrEmpty(node.TempPath))
                node.TempPath = @"d:\videos\temp";
            if (string.IsNullOrEmpty(node.LoggingPath))
                node.LoggingPath = @"d:\videos\logging";

            if (node?.Enabled != true)
            {
                Logger.Instance?.DLog("Flow executor not enabled");
                return;
            }
            if(node.Threads == 0)
                node.Threads = 1;

            if (node.Threads <= ExecutingRunners.Count)
            {
                Logger.Instance?.DLog("At limit of running executors: " + node.Threads);
                return; // already maximum executors running
            }

            string tempPath = node.TempPath;
            if (string.IsNullOrEmpty(tempPath) || Directory.Exists(tempPath) == false)
            {
                Logger.Instance?.ELog("Temp Path not set, cannot process");
                return;
            }

            var libFileService = LibraryFileService.Load();
            var libFile = libFileService.GetNext(node.Uid, Uid).Result;
            if (libFile == null)
                return; // nothing to process

            try
            {
                var libfileService = LibraryFileService.Load();
                var flowService = FlowService.Load();
                FileInfo file = new FileInfo(libFile.Name);
                if (file.Exists == false)
                {
                    libfileService.Delete(libFile.Uid).Wait();
                    return;
                }
                var libService = LibraryService.Load();
                var lib = libService.Get(libFile.Library.Uid).Result;
                if (lib == null)
                {
                    libfileService.Delete(libFile.Uid).Wait();
                    return;
                }

                var flow = flowService.Get(lib.Flow?.Uid ?? Guid.Empty).Result;
                if (flow == null || flow.Uid == Guid.Empty)
                {
                    libFile.Status = FileStatus.FlowNotFound;
                    libfileService.Update(libFile).Wait();
                    return;
                }

                // update the library file to reference the updated flow (if changed)
                if (libFile.Flow.Name != flow.Name || libFile.Flow.Uid != flow.Uid)
                {
                    libFile.Flow = new ObjectReference
                    {
                        Uid = flow.Uid,
                        Name = flow.Name,
                        Type = typeof(Flow)?.FullName ?? String.Empty
                    };
                    libfileService.Update(libFile).Wait();
                }

                Logger.Instance?.ILog("############################# PROCESSING:  " + file.FullName);
                libFile.ProcessingStarted = DateTime.UtcNow;
                libfileService.Update(libFile).Wait();

                var info = new FlowExecutorInfo
                {
                    LibraryFile = libFile,
                    Log = String.Empty,
                    NodeUid = node.Uid,
                    RelativeFile = libFile.RelativePath,
                    Library = libFile.Library,
                    TotalParts = flow.Parts.Count,
                    CurrentPart = 0,
                    CurrentPartPercent = 0,
                    CurrentPartName = string.Empty,
                    StartedAt = DateTime.UtcNow,
                    WorkingFile = libFile.Name
                };

                var runner = new FlowRunner(info, flow, node);
                lock (ExecutingRunners)
                {
                    ExecutingRunners.Add(runner);
                }
                runner.OnFlowCompleted += Runner_OnFlowCompleted;
                _ = runner.Run();
            }
            finally
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1_000);
                    Trigger();
                });
            }
        }

        private void Runner_OnFlowCompleted(FlowRunner sender, bool success)
        {
            lock (this.ExecutingRunners)
            {
                if(ExecutingRunners.Contains(sender))
                    ExecutingRunners.Remove(sender);
            }
        }
    }
}
