using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.TeamFoundation.Common;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Text;

using AutoStateTransitions.Models;
using AutoStateTransitions.Repos;
using AutoStateTransitions.ViewModels;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using AutoStateTransitions.Misc;

namespace AutoStateTransitions.Controllers
{
    [Route("api/reciever")]
    [ApiController]
    public class RecieverController : ControllerBase
    {
        IWorkItemRepo _workItemRepo;
        IOptions<AppSettings> _appSettings;
        IHelper _helper;

        public RecieverController(IWorkItemRepo workItemRepo, IHelper helper, IOptions<AppSettings> appSettings)
        {
            _workItemRepo = workItemRepo;
            _appSettings = appSettings;
            _helper = helper;
        }

        [HttpPost]
        [Route("webhook/post/task")]
        public IActionResult Post([FromBody] JObject payload)
        {
            Int32 parentId;

            PayloadViewModel vm = this.BuildPayloadViewModel(payload);

            //make sure pat is not empty, if it is, pull from appsettings
            vm.pat = _appSettings.Value.PersonalAccessToken;

            //if the event type is something other the updated, then lets just return an ok
            if (vm.eventType != "workitem.updated")
            {
                return new OkResult();
            }

            // create our azure devops connection
            Uri baseUri = new Uri("https://dev.azure.com/" + vm.organization);

            VssCredentials clientCredentials = new VssCredentials(new VssBasicCredential("username", vm.pat));
            VssConnection vssConnection = new VssConnection(baseUri, clientCredentials);

            // load the work item posted 
            WorkItem workItem = this._workItemRepo.GetWorkItem(vssConnection, vm.workItemId);

            // get the related parent
            WorkItemRelation parentRelation = workItem.Relations.Where<WorkItemRelation>(x => x.Rel.Equals("System.LinkTypes.Hierarchy-Reverse")).FirstOrDefault();
            parentId = this._helper.GetWorkItemIdFromUrl(parentRelation.Url);
            WorkItem parentWorkItem = this._workItemRepo.GetWorkItem(vssConnection, parentId);

            string parentState = parentWorkItem.Fields["System.State"] == null ? string.Empty : parentWorkItem.Fields["System.State"].ToString().ToLower();

            // if a child item is being saved as active
            if (vm.state.ToLower().Equals("active"))
            {
                // if the parent of item child is not active or resolved, then save it to active
                if ((!parentState.Equals("active") || parentState.Equals("resolved")))
                {
                    this._workItemRepo.UpdateWorkItemState(vssConnection, parentWorkItem, "Active");
                }

                return new OkResult();
            }

            // if we set the task back to new
            if (vm.state.ToLower().Equals("new"))
            {
                // if the parent is closed, then we need to re-open it as active
                if (parentState.Equals("closed"))
                {
                    this._workItemRepo.UpdateWorkItemState(vssConnection, parentWorkItem, "Active");
                }

                return new OkResult();
            }

            // if we set the task to closed
            if (vm.state.ToLower().Equals("closed"))
            {
                // get a list of all the child items to see if they are all closed or not
                List<WorkItem> childWorkItems = this._workItemRepo.ListChildWorkItemsForParent(vssConnection, parentWorkItem);

                // check to see if any of the child items are not closed, if so, we will get a count > 0
                int count = childWorkItems.Where(x => !x.Fields["System.State"].ToString().ToLower().Equals("closed")).ToList().Count;

                if (count.Equals(0))
                {
                    this._workItemRepo.UpdateWorkItemState(vssConnection, parentWorkItem, "Closed");
                }

                return new OkResult();
            }

            return new StandardResponseObjectResult("temp", StatusCodes.Status200OK);
        }

        private PayloadViewModel BuildPayloadViewModel(JObject body)
        {
            PayloadViewModel vm = new PayloadViewModel();

            string url = body["resource"]["url"] == null ? null : body["resource"]["url"].ToString();
            string org = GetOrganization(url);

            vm.workItemId = body["resource"]["workItemId"] == null ? -1 : Convert.ToInt32(body["resource"]["workItemId"].ToString());
            vm.eventType = body["eventType"] == null ? null : body["eventType"].ToString();
            vm.rev = body["resource"]["rev"] == null ? -1 : Convert.ToInt32(body["resource"]["rev"].ToString());
            vm.url = body["resource"]["url"] == null ? null : body["resource"]["url"].ToString();
            vm.organization = org;
            vm.teamProject = body["resource"]["fields"]["System.AreaPath"] == null ? null : body["resource"]["fields"]["System.AreaPath"].ToString();
            vm.state = body["resource"]["fields"]["System.State"]["newValue"] == null ? null : body["resource"]["fields"]["System.State"]["newValue"].ToString();

            return vm;
        }

        private string GetOrganization(string url)
        {
            url = url.Replace("http://", string.Empty);
            url = url.Replace("https://", string.Empty);

            if (url.Contains(value: "visualstudio.com"))
            {
                string[] split = url.Split('.');
                return split[0].ToString();
            }

            if (url.Contains("dev.azure.com"))
            {
                url = url.Replace("dev.azure.com/", string.Empty);
                string[] split = url.Split('/');
                return split[0].ToString();
            }

            return string.Empty;
        }
    }
}
