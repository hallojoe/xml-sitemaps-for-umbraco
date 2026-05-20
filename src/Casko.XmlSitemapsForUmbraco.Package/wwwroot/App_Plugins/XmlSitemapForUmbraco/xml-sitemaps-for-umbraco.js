import { UMB_ADVANCED_SETTINGS_MENU_ALIAS as t } from "@umbraco-cms/backoffice/settings";
import { UMB_WORKSPACE_CONDITION_ALIAS as i } from "@umbraco-cms/backoffice/workspace";
const o = [
  {
    name: "Umbraco Xml Sitemaps Entrypoint",
    alias: "Casko.XmlSitemapsForUmbraco.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-BSlTz4-p.js")
  }
], e = "Casko.XmlSitemapsForUmbraco.Workspace.XmlSitemaps", a = "casko-xml-sitemaps", m = [
  {
    type: "workspace",
    kind: "default",
    name: "XML Sitemaps Workspace",
    alias: e,
    meta: {
      entityType: a,
      headline: "XML Sitemaps"
    }
  },
  {
    type: "workspaceView",
    name: "XML Sitemaps Configuration Workspace View",
    alias: "Casko.XmlSitemapsForUmbraco.WorkspaceView.Configuration",
    element: () => import("./configuration-workspace-view.element-CDSz4-ZH.js"),
    weight: 500,
    meta: {
      label: "Configuration",
      pathname: "configuration",
      icon: "icon-list"
    },
    conditions: [
      {
        alias: i,
        match: e
      }
    ]
  },
  {
    type: "menuItem",
    name: "XML Sitemap Settings Menu Item",
    alias: "Casko.XmlSitemapsForUmbraco.MenuItem.XmlSitemap",
    weight: 500,
    meta: {
      label: "XML Sitemap",
      icon: "icon-code",
      entityType: a,
      menus: [t]
    }
  }
], p = [
  ...o,
  ...m
];
export {
  p as manifests
};
//# sourceMappingURL=xml-sitemaps-for-umbraco.js.map
