﻿<%@ Page Language="C#" Inherits="SiteServer.BackgroundPages.Cms.PageConfigurationCreateTrigger" %>
  <%@ Register TagPrefix="ctrl" Namespace="SiteServer.BackgroundPages.Controls" Assembly="SiteServer.BackgroundPages" %>
    <!DOCTYPE html>
    <html>

    <head>
      <meta charset="utf-8">
      <!--#include file="../inc/head.html"-->
    </head>

    <body>
      <form class="m-l-15 m-r-15" runat="server">
        <ctrl:alerts runat="server" />

        <div class="card-box">
          <ul class="nav nav-pills">
            <li class="nav-item">
              <a class="nav-link" href="pageConfigurationCreateRule.aspx?publishmentSystemId=<%=PublishmentSystemId%>">页面命名规则</a>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="pageConfigurationCreate.aspx?publishmentSystemId=<%=PublishmentSystemId%>">页面生成设置</a>
            </li>
            <li class="nav-item active">
              <a class="nav-link" href="javascript:;">页面生成触发器</a>
            </li>
          </ul>
        </div>

        <div class="card-box">

          <div class="panel panel-default">
            <div class="panel-body p-0">
              <div class="table-responsive">
                <table class="tablesaw table table-hover m-b-0">
                  <thead>
                    <tr>
                      <th>栏目名</th>
                      <th>内容变动时需要生成的栏目</th>
                      <th class="center" style="width:120px;">&nbsp;</th>
                    </tr>
                  </thead>
                  <tbody>
                    <asp:Repeater ID="RptContents" runat="server">
                      <itemtemplate>
                        <asp:Literal id="ltlHtml" runat="server" />
                      </itemtemplate>
                    </asp:Repeater>
                  </tbody>
                </table>
              </div>
            </div>
          </div>

        </div>

      </form>
    </body>

    </html>