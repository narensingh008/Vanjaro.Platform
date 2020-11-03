﻿using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Components;
using Vanjaro.Core.Components.Interfaces;
using Vanjaro.Core.Data.Entities;
using Vanjaro.Core.Extensions.Workflow.Review.Components;
using static Vanjaro.Core.Components.Enum;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Extensions.Workflow.Review.Managers
{
    public class ModeratorManager
    {
        internal static Dictionary<string, IUIData> GetData(PortalSettings PortalSettings, int Version, string Entity, int EntityID)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();

            Dictionary<string, object> Data = GenerateMarkup(Version, Entity, EntityID, PortalSettings.UserInfo);
            Settings.Add("Markup", new UIData { Name = "Markup", Value = Data["Markup"].ToString() });
            Settings.Add("WorkflowType", new UIData { Name = "WorkflowType", Value = Data["WorkflowType"].ToString() });
            Settings.Add("ReviewContentInfo", new UIData { Name = "ReviewContentInfo", Options = Data["ReviewContentInfo"] });
            Settings.Add("PreviousState", new UIData { Name = "PreviousState", Options = Data["PreviousState"] });
            Settings.Add("CurrentState", new UIData { Name = "CurrentState", Options = Data["CurrentState"] });
            Settings.Add("NextState", new UIData { Name = "NextState", Options = Data["NextState"] });
            Settings.Add("LastState", new UIData { Name = "LastState", Options = Data["LastState"] });
            return Settings;
        }
        internal static Dictionary<string, object> GenerateMarkup(int Version, string Entity, int EntityID, UserInfo UserInfo)
        {

            ReviewContentInfo rinfo = Core.Factories.PageFactory.GetReviewContentInfo(Version, Entity, EntityID, UserInfo);


            Dictionary<string, object> Data = new Dictionary<string, object>();
            string Markup = string.Empty;
            WorkflowState wState = new WorkflowState();
            if (rinfo != null)
            {
                wState = WorkflowManager.GetStateByID(rinfo.StateID);
                string tabtest = string.Empty;
                bool hasperm = WorkflowManager.HasReviewPermission(wState.StateID, UserInfo);
                List<WorkflowLog> PageWorkflowLogs = WorkflowManager.GetEntityWorkflowLogs(Entity, rinfo.EntityID, rinfo.Version).OrderBy(a => a.ReviewedOn).ToList();
                if (PageWorkflowLogs.Count > 0)
                {

                    int Counter = 0;
                    foreach (WorkflowLog wlog in PageWorkflowLogs.OrderByDescending(a => a.ReviewedOn))
                    {
                        UserInfo uf = UserController.GetUserById(UserInfo.PortalID, wlog.ReviewedBy);
                        if (uf != null)
                        {
                            Counter++;
                            string IsAppRej = string.Empty;
                            bool Approved = wlog.Approved;
                            string workflowstatename = string.Empty;
                            WorkflowState wstate = WorkflowManager.GetStateByID(wlog.StateID);
                            if (wstate != null)
                            {
                                workflowstatename = wstate.Name;
                            }

                            if (Approved)
                            {
                                IsAppRej = "<span class=\"badge font-sm badge-success\">" + workflowstatename + "</span>";
                            }
                            else
                            {
                                IsAppRej = "<span class=\"badge font-sm badge-error\">" + workflowstatename + "</span>";
                            }

                            StringBuilder sb = new StringBuilder();
                            sb.Append("<div class=\"revision_block ng-scope\"><div>");
                            sb.Append("<div class=\"row m-none \">");
                            sb.Append("<div class=\"col-8 user_imgname p-none\">");
                            sb.Append("<div class=\"float-left\"><img class=\"preview_pic\" src=\"" + UserUtils.GetProfileImage(UserInfo.PortalID, wlog.ReviewedBy) + "\"></div>");
                            sb.Append("<span class=\"author_name\">" + uf.DisplayName + "</span></div>");
                            sb.Append("<div class=\"col-4 revision_info text-right\">");
                            sb.Append("" + IsAppRej + "");
                            sb.Append("<p class=\"date m-none\"><span>" + wlog.ReviewedOn + " </span></p></div></div>");
                            sb.Append("<div class=\"row m-none \"><div class=\"col-12 commentInfo\"><p>" + wlog.Comment + "</p></div></div></div></div>");

                            tabtest += sb.ToString();
                        }

                    }

                }



                if (!string.IsNullOrEmpty(tabtest))
                {
                    Markup = "<div>" + tabtest + "<div style=\"clear:both\"></div></div>";
                }
            }
            Data.Add("Markup", Markup);
            Data.Add("WorkflowType", WorkflowManager.GetWorkflowType(wState.WorkflowID));
            Data.Add("ReviewContentInfo", rinfo);
            Data.Add("PreviousState", WorkflowManager.GetStateByID(WorkflowManager.GetPreviousStateID(wState.WorkflowID, wState.StateID)));
            Data.Add("CurrentState", wState);
            Data.Add("NextState", WorkflowManager.GetStateByID(WorkflowManager.GetNextStateID(wState.WorkflowID, wState.StateID)));
            Data.Add("LastState", WorkflowManager.GetStateByID(WorkflowManager.GetLastStateID(wState.WorkflowID).StateID));
            return Data;
        }
    }
}