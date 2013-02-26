﻿using MediaBrowser.Common.Kernel;
using MediaBrowser.Common.ScheduledTasks;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Common.Implementations.ScheduledTasks.Tasks
{
    /// <summary>
    /// Class ReloadLoggerFileTask
    /// </summary>
    public class ReloadLoggerFileTask : IScheduledTask
    {
        /// <summary>
        /// Gets or sets the log manager.
        /// </summary>
        /// <value>The log manager.</value>
        private ILogManager LogManager { get; set; }
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; set; }
        /// <summary>
        /// Gets or sets the kernel.
        /// </summary>
        /// <value>The kernel.</value>
        private IKernel Kernel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReloadLoggerFileTask" /> class.
        /// </summary>
        /// <param name="logManager">The logManager.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="kernel">The kernel.</param>
        public ReloadLoggerFileTask(ILogManager logManager, ILogger logger, IKernel kernel)
        {
            LogManager = logManager;
            Logger = logger;
            Kernel = kernel;
        }

        /// <summary>
        /// Gets the default triggers.
        /// </summary>
        /// <returns>IEnumerable{BaseTaskTrigger}.</returns>
        public IEnumerable<ITaskTrigger> GetDefaultTriggers()
        {
            var trigger = new DailyTrigger { TimeOfDay = TimeSpan.FromHours(0) }; //12am

            return new[] { trigger };
        }

        /// <summary>
        /// Executes the internal.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>Task.</returns>
        public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            cancellationToken.ThrowIfCancellationRequested();

            progress.Report(0);

            return Task.Run(() => LogManager.ReloadLogger(Kernel.Configuration.EnableDebugLevelLogging ? LogSeverity.Debug : LogSeverity.Info));
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Start new log file"; }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return "Moves logging to a new file to help reduce log file sizes."; }
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>The category.</value>
        public string Category
        {
            get { return "Application"; }
        }
    }
}
