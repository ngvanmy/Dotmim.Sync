﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.Web.Server;
using Microsoft.AspNetCore.Mvc;

namespace Dotmim.Sync.SampleWebServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private WebServerManager webServerManager;

        // Injected thanks to Dependency Injection
        public SyncController(WebServerManager webServerManager) => this.webServerManager = webServerManager;

        [HttpPost]
        public async Task Post()
        {
            try
            {
                // Get Orchestrator regarding the incoming scope name (from http context)
                var orchestrator = webServerManager.GetOrchestrator(this.HttpContext);

                orchestrator.OnApplyChangesFailed(e =>
                {
                    if (e.Conflict.RemoteRow.Table.TableName == "Region")
                    {
                        e.Resolution = ConflictResolution.MergeRow;
                        e.FinalRow["RegionDescription"] = "Eastern alone !";
                    }
                    else
                    {
                        e.Resolution = ConflictResolution.ServerWins;
                    }
                });

                var progress = new SynchronousProgress<ProgressArgs>(pa => Debug.WriteLine($"{pa.Context.SyncStage}\t {pa.Message}"));

                // handle request
                await webServerManager.HandleRequestAsync(this.HttpContext, default, progress);

            }
            catch (Exception ex)
            {
                await WebServerManager.WriteExceptionAsync(this.HttpContext.Response, ex);
            }
        }

        /// <summary>
        /// This Get handler is optional. It allows you to see the configuration hosted on the server
        /// The configuration is shown only if Environmenent == Development
        /// </summary>
        [HttpGet]
        public async Task Get() => await webServerManager.HandleRequestAsync(this.HttpContext);
    }
}
