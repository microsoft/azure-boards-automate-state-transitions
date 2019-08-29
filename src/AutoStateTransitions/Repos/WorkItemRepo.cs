using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

using AutoStateTransitions.Misc;
using AutoStateTransitions.Models;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;

namespace AutoStateTransitions.Repos
{
    public class WorkItemRepo : IWorkItemRepo, IDisposable
    {
        private IOptions<AppSettings> _appSettings;
        private IHelper _helper;

        public WorkItemRepo(IOptions<AppSettings> appSettings, IHelper helper)
        {
            _appSettings = appSettings;
            _helper = helper;
        }

        public WorkItem GetWorkItem(VssConnection connection, int id)
        {
            WorkItemTrackingHttpClient client = connection.GetClient<WorkItemTrackingHttpClient>();

            try
            {
                WorkItem item = client.GetWorkItemAsync(id, null, null, WorkItemExpand.Relations).Result;

                return item;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<WorkItem> ListChildWorkItemsForParent(VssConnection connection, WorkItem parentWorkItem)
        {
            WorkItemTrackingHttpClient client = connection.GetClient<WorkItemTrackingHttpClient>();

            // get all the related child work item links
            IEnumerable<WorkItemRelation> children = parentWorkItem.Relations.Where<WorkItemRelation>(x => x.Rel.Equals("System.LinkTypes.Hierarchy-Forward"));
            IList<int> Ids = new List<int>();

            // loop through children and extract the id's the from the url
            foreach (var child in children)
            {
                Ids.Add(this._helper.GetWorkItemIdFromUrl(child.Url));
            }

            // in this case we only care about the state of the child work items
            string[] fields = new string[] { "System.State" };

            // go get the full list of child work items with the desired fields
            List<WorkItem> list = client.GetWorkItemsAsync(Ids, fields).Result;

            return list;
        }

        public WorkItem UpdateWorkItemState(VssConnection connection, WorkItem workItem, string state)
        {
            JsonPatchDocument patchDocument = new JsonPatchDocument();

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Test,
                    Path = "/rev",
                    Value = workItem.Rev.ToString()
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.State",
                    Value = state
                }
            );

            WorkItemTrackingHttpClient client = connection.GetClient<WorkItemTrackingHttpClient>();
            WorkItem result = null;

            try
            {
                result = client.UpdateWorkItemAsync(patchDocument, Convert.ToInt32(workItem.Id)).Result;
            }
            catch (Exception ex)
            {
                result = null;
            }
            finally
            {
                connection = null;
                client = null;
            }

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WorkItemRepo()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _appSettings = null;
            }
        }
    }

    public interface IWorkItemRepo
    {
        WorkItem GetWorkItem(VssConnection connection, int id);
        List<WorkItem> ListChildWorkItemsForParent(VssConnection connection, WorkItem parentWorkItem);
        WorkItem UpdateWorkItemState(VssConnection connection, WorkItem workItem, string state);
    }
}
