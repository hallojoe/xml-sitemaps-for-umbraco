import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { css, customElement, html, state } from "@umbraco-cms/backoffice/external/lit";
import { V1Service } from "../api/sdk.gen.js";
import type {
  XmlSitemapConfigurationResponse,
  XmlSitemapConfigurationRowResponse,
  XmlSitemapCustomConfigurationRowResponse,
  XmlSitemapIndexConfigurationRowResponse,
} from "../api/types.gen.js";

type SitemapReferenceRow = {
  key?: string | null;
  publicName?: string | null;
  hostName?: string | null;
};

type SitemapIndexReferenceRow = SitemapReferenceRow & {
  publicSitemaps?: Array<string>;
};

@customElement("casko-xml-sitemaps-configuration-workspace-view")
export class CaskoXmlSitemapsConfigurationWorkspaceViewElement extends UmbLitElement {
  @state()
  private _configuration?: XmlSitemapConfigurationResponse;

  @state()
  private _error?: string;

  @state()
  private _isLoading = true;

  connectedCallback() {
    super.connectedCallback();
    void this._loadConfiguration();
  }

  override render() {
    if (this._isLoading) {
      return html`<uui-box headline="XML Sitemaps"><uui-loader></uui-loader></uui-box>`;
    }

    if (this._error) {
      return html`
        <uui-box headline="XML Sitemaps">
          <div class="state state-error">${this._error}</div>
        </uui-box>
      `;
    }

    const configuration = this._configuration;
    if (!configuration) {
      return html`
        <uui-box headline="XML Sitemaps">
          <div class="state">No configuration was returned.</div>
        </uui-box>
      `;
    }

    return html`
      <uui-box headline="XML Sitemaps">
        <div class="status-row">
          <span class=${configuration.enabled ? "status enabled" : "status disabled"}>
            ${configuration.enabled ? "Enabled" : "Disabled"}
          </span>
          <span class="muted">Rewrites ${this._formatBoolean(configuration.rewritesEnabled)}</span>
          <span class="muted">
            Alternate links ${this._formatBoolean(configuration.renderAlternateLinksForSingleCultureSitemaps)}
          </span>
        </div>

        <dl class="summary-grid">
          ${this._renderSummaryItem("Configured sitemaps", configuration.sitemapCount)}
          ${this._renderSummaryItem("Custom sitemaps", configuration.customSitemapCount)}
          ${this._renderSummaryItem("Indexes", configuration.indexCount)}
          ${this._renderSummaryItem("Root search level", configuration.rootNodeSearchLevel)}
          ${this._renderSummaryItem("Version cleanup", this._formatSeconds(configuration.storage?.versionCleanupAfterSeconds))}
          ${this._renderSummaryItem("Background job", this._formatBoolean(configuration.storage?.backgroundJobEnabled))}
          ${this._renderSummaryItem("Job interval", this._formatSeconds(configuration.storage?.backgroundJobIntervalSeconds))}
        </dl>
      </uui-box>

      <uui-box headline="Global filters">
        <div class="detail-grid">
          ${this._renderListDetail("Included cultures", configuration.globalFilters?.includedCultures)}
          ${this._renderListDetail("Excluded cultures", configuration.globalFilters?.excludedCultures)}
          ${this._renderListDetail("Included content types", configuration.globalFilters?.includedContentTypeAliases, "All")}
          ${this._renderListDetail("Excluded content types", configuration.globalFilters?.excludedContentTypeAliases)}
          ${this._renderDetail("Excluding URL property", configuration.globalFilters?.excludingUrlPropertyAlias)}
          ${this._renderDetail("Excluding URL value", configuration.globalFilters?.excludingUrlPropertyValue)}
        </div>
      </uui-box>

      ${this._renderConfiguredSitemaps(configuration.sitemaps ?? [], configuration.rewritesEnabled)}
      ${this._renderCustomSitemaps(configuration.customSitemaps ?? [], configuration.rewritesEnabled)}
      ${this._renderIndexes(configuration.indexes ?? [], configuration.rewritesEnabled)}
    `;
  }

  private async _loadConfiguration() {
    try {
      const result = await V1Service.getConfiguration({ throwOnError: true });
      this._configuration = result.data as XmlSitemapConfigurationResponse;
    } catch {
      this._error = "Unable to load sitemap configuration.";
    } finally {
      this._isLoading = false;
    }
  }

  private _renderConfiguredSitemaps(rows: Array<XmlSitemapConfigurationRowResponse>, rewritesEnabled: boolean) {
    return this._renderTable(
      "Configured Sitemaps",
      rows,
      html`
        <tr>
          <th>Key</th>
          <th>Public name</th>
          <th>Host</th>
          <th>Path</th>
          <th>Culture</th>
          <th>Included cultures</th>
          <th>Excluded cultures</th>
          <th>Included types</th>
          <th>Excluded types</th>
        </tr>
      `,
      (row) => html`
        <tr>
          <td>${this._formatValue(row.key)}</td>
          <td>${this._renderSitemapReference(this._getPublicName(row), row.hostName, rewritesEnabled)}</td>
          <td>${this._formatValue(row.hostName)}</td>
          <td>${this._formatValue(row.path)}</td>
          <td>${this._formatValue(row.culture)}</td>
          <td>${this._formatList(row.includedCultures)}</td>
          <td>${this._formatList(row.excludedCultures)}</td>
          <td>${this._formatList(row.includedDocumentTypeAliases, "All")}</td>
          <td>${this._formatList(row.excludedDocumentTypeAliases)}</td>
        </tr>
      `,
      "No configured sitemaps."
    );
  }

  private _renderCustomSitemaps(rows: Array<XmlSitemapCustomConfigurationRowResponse>, rewritesEnabled: boolean) {
    return this._renderTable(
      "Custom Sitemaps",
      rows,
      html`
        <tr>
          <th>Key</th>
          <th>Public name</th>
          <th>Provider</th>
          <th>Host</th>
          <th>Settings</th>
        </tr>
      `,
      (row) => html`
        <tr>
          <td>${this._formatValue(row.key)}</td>
          <td>${this._renderSitemapReference(this._getPublicName(row), row.hostName, rewritesEnabled)}</td>
          <td>${this._formatValue(row.providerAlias)}</td>
          <td>${this._formatValue(row.hostName)}</td>
          <td>${this._formatSettingKeys(row.settingKeys, row.settingCount)}</td>
        </tr>
      `,
      "No custom sitemaps."
    );
  }

  private _renderIndexes(rows: Array<XmlSitemapIndexConfigurationRowResponse>, rewritesEnabled: boolean) {
    return this._renderTable(
      "Indexes",
      rows,
      html`
        <tr>
          <th>Key</th>
          <th>Public name</th>
          <th>Host</th>
          <th>Sitemaps</th>
        </tr>
      `,
      (row) => html`
        <tr>
          <td>${this._formatValue(row.key)}</td>
          <td>${this._renderSitemapReference(this._getPublicName(row), row.hostName, rewritesEnabled)}</td>
          <td>${this._formatValue(row.hostName)}</td>
          <td>${this._renderSitemapReferenceList(this._getPublicSitemaps(row), row.hostName, rewritesEnabled)}</td>
        </tr>
      `,
      "No sitemap indexes."
    );
  }

  private _renderTable<T>(headline: string, rows: Array<T>, header: unknown, rowTemplate: (row: T) => unknown, emptyText: string) {
    return html`
      <uui-box headline=${headline}>        

        ${rows.length === 0
          ? html`<div class="state">${emptyText}</div>`
          : html`
              <div class="table-wrap">
                <table>
                  <thead>
                    ${header}
                  </thead>
                  <tbody>
                    ${rows.map((row) => rowTemplate(row))}
                  </tbody>
                </table>
              </div>
            `}
      </uui-box>
    `;
  }

  private _renderSummaryItem(label: string, value: string | number | undefined | null) {
    return html`
      <div>
        <dt>${label}</dt>
        <dd>${this._formatValue(value)}</dd>
      </div>
    `;
  }

  private _renderListDetail(label: string, values?: Array<string>, message:string = "None") {
    return html`
      <div class="detail">
        <span>${label}</span>
        <strong>${this._formatList(values, message)}</strong>
      </div>
    `;
  }

  private _renderDetail(label: string, value?: string | number | null) {
    return html`
      <div class="detail">
        <span>${label}</span>
        <strong>${this._formatValue(value)}</strong>
      </div>
    `;
  }

  private _renderSitemapReference(key: string | undefined | null, hostName: string | undefined | null, rewritesEnabled: boolean) {
    if (!rewritesEnabled || !key) {
      return this._formatValue(key);
    }

    return html`
      <a class="sitemap-link" href=${this._buildSitemapUrl(key, hostName)} target="_blank" rel="noopener noreferrer">
        ${key}
      </a>
    `;
  }

  private _renderSitemapReferenceList(values: Array<string> | undefined, hostName: string | undefined | null, rewritesEnabled: boolean) {
    if (!values || values.length === 0) {
      return "None";
    }

    if (!rewritesEnabled) {
      return this._formatList(values);
    }

    return html`
      <span class="sitemap-link-list">
        ${values.map(
          (value, index) => html`
            ${index > 0 ? html`<span>, </span>` : undefined}
            ${this._renderSitemapReference(value, hostName, rewritesEnabled)}
          `
        )}
      </span>
    `;
  }

  private _getPublicName(row: SitemapReferenceRow) {
    return row.publicName || row.key;
  }

  private _getPublicSitemaps(row: XmlSitemapIndexConfigurationRowResponse) {
    const indexRow = row as SitemapIndexReferenceRow;
    return indexRow.publicSitemaps && indexRow.publicSitemaps.length > 0
      ? indexRow.publicSitemaps
      : row.sitemaps;
  }

  private _buildSitemapUrl(key: string, hostName: string | undefined | null) {
    const path = `/${encodeURIComponent(key)}.xml`;
    const host = hostName?.trim();

    if (!host) {
      return path;
    }

    const normalizedHost = host.replace(/\/+$/, "");
    if (/^https?:\/\//i.test(normalizedHost)) {
      return `${normalizedHost}${path}`;
    }

    return `https://${normalizedHost}${path}`;
  }

  private _formatSettingKeys(values?: Array<string>, count?: number) {
    const label = `${count ?? values?.length ?? 0} configured`;
    return values && values.length > 0 ? `${label}: ${values.join(", ")}` : label;
  }

  private _formatSeconds(value?: number | null) {
    return value === undefined || value === null ? "Not configured" : `${value} sec`;
  }

  private _formatBoolean(value?: boolean) {
    return value ? "enabled" : "disabled";
  }

  private _formatList(values?: Array<string>, message:string = "None") {
    return values && values.length > 0 ? values.join(", ") : message;
  }

  private _formatValue(value: string | number | undefined | null) {
    return value === undefined || value === null || value === "" ? "Not configured" : value;
  }

  static override styles = [
    css`
      :host {
        display: grid;
        gap: var(--uui-size-layout-1);
        padding: var(--uui-size-layout-1);
      }

      .status-row {
        align-items: center;
        display: flex;
        flex-wrap: wrap;
        gap: var(--uui-size-space-4);
      }

      .status {
        border-radius: 999px;
        display: inline-flex;
        font-weight: 700;
        line-height: 1;
        padding: var(--uui-size-space-3) var(--uui-size-space-4);
      }

      .enabled {
        background: var(--uui-color-positive);
        color: var(--uui-color-positive-contrast);
      }

      .disabled {
        background: var(--uui-color-danger);
        color: var(--uui-color-danger-contrast);
      }

      .muted {
        color: var(--uui-color-text-alt);
      }

      .summary-grid,
      .detail-grid {
        display: grid;
        gap: var(--uui-size-space-4);
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        margin: var(--uui-size-layout-1) 0 0;
      }

      .summary-grid div,
      .detail {
        border: 1px solid var(--uui-color-border);
        border-radius: var(--uui-border-radius);
        padding: var(--uui-size-space-4);
      }

      dt,
      .detail span {
        color: var(--uui-color-text-alt);
        font-size: 0.875rem;
      }

      dd {
        font-size: 1.25rem;
        font-weight: 700;
        margin: var(--uui-size-space-2) 0 0;
      }

      .detail strong {
        display: block;
        margin-top: var(--uui-size-space-2);
        overflow-wrap: anywhere;
      }

      .section {
        background: var(--uui-color-surface);
        border-radius: var(--uui-border-radius);
        padding: var(--uui-size-layout-1);
      }

      h2 {
        font-size: 1rem;
        margin: 0 0 var(--uui-size-space-4);
      }

      .state {
        color: var(--uui-color-text-alt);
        padding: var(--uui-size-space-4) 0;
      }

      .state-error {
        color: var(--uui-color-danger);
      }

      .table-wrap {
        overflow-x: auto;
      }

      table {
        border-collapse: collapse;
        min-width: 900px;
        width: 100%;
      }

      th,
      td {
        border-bottom: 1px solid var(--uui-color-border);
        padding: var(--uui-size-space-4);
        text-align: left;
        vertical-align: top;
      }

      th {
        color: var(--uui-color-text-alt);
        font-size: 0.8125rem;
        font-weight: 700;
      }

      td {
        overflow-wrap: anywhere;
      }

      .sitemap-link {
        color: var(--uui-color-interactive);
        text-decoration: none;
        overflow-wrap: anywhere;
      }

      .sitemap-link:hover {
        color: var(--uui-color-interactive-emphasis);
        text-decoration: underline;
      }

      .sitemap-link-list {
        overflow-wrap: anywhere;
      }
    `,
  ];
}

export default CaskoXmlSitemapsConfigurationWorkspaceViewElement;
