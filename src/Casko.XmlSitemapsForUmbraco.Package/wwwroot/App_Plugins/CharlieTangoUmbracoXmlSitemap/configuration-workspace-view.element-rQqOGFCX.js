import { UmbLitElement as G } from "@umbraco-cms/backoffice/lit-element";
import { html as p, css as Q, state as O, customElement as Y } from "@umbraco-cms/backoffice/external/lit";
import { umbHttpClient as Z } from "@umbraco-cms/backoffice/http-client";
const ee = {
  bodySerializer: (e) => JSON.stringify(
    e,
    (t, r) => typeof r == "bigint" ? r.toString() : r
  )
}, te = ({
  onRequest: e,
  onSseError: t,
  onSseEvent: r,
  responseTransformer: i,
  responseValidator: a,
  sseDefaultRetryDelay: l,
  sseMaxRetryAttempts: o,
  sseMaxRetryDelay: n,
  sseSleepFn: d,
  url: c,
  ...s
}) => {
  let f;
  const C = d ?? ((u) => new Promise((m) => setTimeout(m, u)));
  return { stream: async function* () {
    let u = l ?? 3e3, m = 0;
    const v = s.signal ?? new AbortController().signal;
    for (; !v.aborted; ) {
      m++;
      const z = s.headers instanceof Headers ? s.headers : new Headers(s.headers);
      f !== void 0 && z.set("Last-Event-ID", f);
      try {
        const _ = {
          redirect: "follow",
          ...s,
          body: s.serializedBody,
          headers: z,
          signal: v
        };
        let g = new Request(c, _);
        e && (g = await e(c, _));
        const b = await (s.fetch ?? globalThis.fetch)(g);
        if (!b.ok)
          throw new Error(
            `SSE failed: ${b.status} ${b.statusText}`
          );
        if (!b.body) throw new Error("No body in SSE response");
        const S = b.body.pipeThrough(new TextDecoderStream()).getReader();
        let A = "";
        const L = () => {
          try {
            S.cancel();
          } catch {
          }
        };
        v.addEventListener("abort", L);
        try {
          for (; ; ) {
            const { done: J, value: K } = await S.read();
            if (J) break;
            A += K;
            const q = A.split(`

`);
            A = q.pop() ?? "";
            for (const M of q) {
              const X = M.split(`
`), j = [];
              let N;
              for (const y of X)
                if (y.startsWith("data:"))
                  j.push(y.replace(/^data:\s*/, ""));
                else if (y.startsWith("event:"))
                  N = y.replace(/^event:\s*/, "");
                else if (y.startsWith("id:"))
                  f = y.replace(/^id:\s*/, "");
                else if (y.startsWith("retry:")) {
                  const B = Number.parseInt(
                    y.replace(/^retry:\s*/, ""),
                    10
                  );
                  Number.isNaN(B) || (u = B);
                }
              let $, D = !1;
              if (j.length) {
                const y = j.join(`
`);
                try {
                  $ = JSON.parse(y), D = !0;
                } catch {
                  $ = y;
                }
              }
              D && (a && await a($), i && ($ = await i($))), r?.({
                data: $,
                event: N,
                id: f,
                retry: u
              }), j.length && (yield $);
            }
          }
        } finally {
          v.removeEventListener("abort", L), S.releaseLock();
        }
        break;
      } catch (_) {
        if (t?.(_), o !== void 0 && m >= o)
          break;
        const g = Math.min(
          u * 2 ** (m - 1),
          n ?? 3e4
        );
        await C(g);
      }
    }
  }() };
}, re = (e) => {
  switch (e) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, ae = (e) => {
  switch (e) {
    case "form":
      return ",";
    case "pipeDelimited":
      return "|";
    case "spaceDelimited":
      return "%20";
    default:
      return ",";
  }
}, se = (e) => {
  switch (e) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, U = ({
  allowReserved: e,
  explode: t,
  name: r,
  style: i,
  value: a
}) => {
  if (!t) {
    const n = (e ? a : a.map((d) => encodeURIComponent(d))).join(ae(i));
    switch (i) {
      case "label":
        return `.${n}`;
      case "matrix":
        return `;${r}=${n}`;
      case "simple":
        return n;
      default:
        return `${r}=${n}`;
    }
  }
  const l = re(i), o = a.map((n) => i === "label" || i === "simple" ? e ? n : encodeURIComponent(n) : k({
    allowReserved: e,
    name: r,
    value: n
  })).join(l);
  return i === "label" || i === "matrix" ? l + o : o;
}, k = ({
  allowReserved: e,
  name: t,
  value: r
}) => {
  if (r == null)
    return "";
  if (typeof r == "object")
    throw new Error(
      "Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these."
    );
  return `${t}=${e ? r : encodeURIComponent(r)}`;
}, H = ({
  allowReserved: e,
  explode: t,
  name: r,
  style: i,
  value: a,
  valueOnly: l
}) => {
  if (a instanceof Date)
    return l ? a.toISOString() : `${r}=${a.toISOString()}`;
  if (i !== "deepObject" && !t) {
    let d = [];
    Object.entries(a).forEach(([s, f]) => {
      d = [
        ...d,
        s,
        e ? f : encodeURIComponent(f)
      ];
    });
    const c = d.join(",");
    switch (i) {
      case "form":
        return `${r}=${c}`;
      case "label":
        return `.${c}`;
      case "matrix":
        return `;${r}=${c}`;
      default:
        return c;
    }
  }
  const o = se(i), n = Object.entries(a).map(
    ([d, c]) => k({
      allowReserved: e,
      name: i === "deepObject" ? `${r}[${d}]` : d,
      value: c
    })
  ).join(o);
  return i === "label" || i === "matrix" ? o + n : n;
}, ie = /\{[^{}]+\}/g, ne = ({ path: e, url: t }) => {
  let r = t;
  const i = t.match(ie);
  if (i)
    for (const a of i) {
      let l = !1, o = a.substring(1, a.length - 1), n = "simple";
      o.endsWith("*") && (l = !0, o = o.substring(0, o.length - 1)), o.startsWith(".") ? (o = o.substring(1), n = "label") : o.startsWith(";") && (o = o.substring(1), n = "matrix");
      const d = e[o];
      if (d == null)
        continue;
      if (Array.isArray(d)) {
        r = r.replace(
          a,
          U({ explode: l, name: o, style: n, value: d })
        );
        continue;
      }
      if (typeof d == "object") {
        r = r.replace(
          a,
          H({
            explode: l,
            name: o,
            style: n,
            value: d,
            valueOnly: !0
          })
        );
        continue;
      }
      if (n === "matrix") {
        r = r.replace(
          a,
          `;${k({
            name: o,
            value: d
          })}`
        );
        continue;
      }
      const c = encodeURIComponent(
        n === "label" ? `.${d}` : d
      );
      r = r.replace(a, c);
    }
  return r;
}, oe = ({
  baseUrl: e,
  path: t,
  query: r,
  querySerializer: i,
  url: a
}) => {
  const l = a.startsWith("/") ? a : `/${a}`;
  let o = (e ?? "") + l;
  t && (o = ne({ path: t, url: o }));
  let n = r ? i(r) : "";
  return n.startsWith("?") && (n = n.substring(1)), n && (o += `?${n}`), o;
};
function de(e) {
  const t = e.body !== void 0;
  if (t && e.bodySerializer)
    return "serializedBody" in e ? e.serializedBody !== void 0 && e.serializedBody !== "" ? e.serializedBody : null : e.body !== "" ? e.body : null;
  if (t)
    return e.body;
}
const le = async (e, t) => {
  const r = typeof t == "function" ? await t(e) : t;
  if (r)
    return e.scheme === "bearer" ? `Bearer ${r}` : e.scheme === "basic" ? `Basic ${btoa(r)}` : r;
}, W = ({
  allowReserved: e,
  array: t,
  object: r
} = {}) => (a) => {
  const l = [];
  if (a && typeof a == "object")
    for (const o in a) {
      const n = a[o];
      if (n != null)
        if (Array.isArray(n)) {
          const d = U({
            allowReserved: e,
            explode: !0,
            name: o,
            style: "form",
            value: n,
            ...t
          });
          d && l.push(d);
        } else if (typeof n == "object") {
          const d = H({
            allowReserved: e,
            explode: !0,
            name: o,
            style: "deepObject",
            value: n,
            ...r
          });
          d && l.push(d);
        } else {
          const d = k({
            allowReserved: e,
            name: o,
            value: n
          });
          d && l.push(d);
        }
    }
  return l.join("&");
}, ue = (e) => {
  if (!e)
    return "stream";
  const t = e.split(";")[0]?.trim();
  if (t) {
    if (t.startsWith("application/json") || t.endsWith("+json"))
      return "json";
    if (t === "multipart/form-data")
      return "formData";
    if (["application/", "audio/", "image/", "video/"].some(
      (r) => t.startsWith(r)
    ))
      return "blob";
    if (t.startsWith("text/"))
      return "text";
  }
}, ce = (e, t) => t ? !!(e.headers.has(t) || e.query?.[t] || e.headers.get("Cookie")?.includes(`${t}=`)) : !1, fe = async ({
  security: e,
  ...t
}) => {
  for (const r of e) {
    if (ce(t, r.name))
      continue;
    const i = await le(r, t.auth);
    if (!i)
      continue;
    const a = r.name ?? "Authorization";
    switch (r.in) {
      case "query":
        t.query || (t.query = {}), t.query[a] = i;
        break;
      case "cookie":
        t.headers.append("Cookie", `${a}=${i}`);
        break;
      default:
        t.headers.set(a, i);
        break;
    }
  }
}, P = (e) => oe({
  baseUrl: e.baseUrl,
  path: e.path,
  query: e.query,
  querySerializer: typeof e.querySerializer == "function" ? e.querySerializer : W(e.querySerializer),
  url: e.url
}), V = (e, t) => {
  const r = { ...e, ...t };
  return r.baseUrl?.endsWith("/") && (r.baseUrl = r.baseUrl.substring(0, r.baseUrl.length - 1)), r.headers = R(e.headers, t.headers), r;
}, he = (e) => {
  const t = [];
  return e.forEach((r, i) => {
    t.push([i, r]);
  }), t;
}, R = (...e) => {
  const t = new Headers();
  for (const r of e) {
    if (!r)
      continue;
    const i = r instanceof Headers ? he(r) : Object.entries(r);
    for (const [a, l] of i)
      if (l === null)
        t.delete(a);
      else if (Array.isArray(l))
        for (const o of l)
          t.append(a, o);
      else l !== void 0 && t.set(
        a,
        typeof l == "object" ? JSON.stringify(l) : l
      );
  }
  return t;
};
class T {
  constructor() {
    this.fns = [];
  }
  clear() {
    this.fns = [];
  }
  eject(t) {
    const r = this.getInterceptorIndex(t);
    this.fns[r] && (this.fns[r] = null);
  }
  exists(t) {
    const r = this.getInterceptorIndex(t);
    return !!this.fns[r];
  }
  getInterceptorIndex(t) {
    return typeof t == "number" ? this.fns[t] ? t : -1 : this.fns.indexOf(t);
  }
  update(t, r) {
    const i = this.getInterceptorIndex(t);
    return this.fns[i] ? (this.fns[i] = r, t) : !1;
  }
  use(t) {
    return this.fns.push(t), this.fns.length - 1;
  }
}
const pe = () => ({
  error: new T(),
  request: new T(),
  response: new T()
}), me = W({
  allowReserved: !1,
  array: {
    explode: !0,
    style: "form"
  },
  object: {
    explode: !0,
    style: "deepObject"
  }
}), be = {
  "Content-Type": "application/json"
}, F = (e = {}) => ({
  ...ee,
  headers: be,
  parseAs: "auto",
  querySerializer: me,
  ...e
}), ye = (e = {}) => {
  let t = V(F(), e);
  const r = () => ({ ...t }), i = (c) => (t = V(t, c), r()), a = pe(), l = async (c) => {
    const s = {
      ...t,
      ...c,
      fetch: c.fetch ?? t.fetch ?? globalThis.fetch,
      headers: R(t.headers, c.headers),
      serializedBody: void 0
    };
    s.security && await fe({
      ...s,
      security: s.security
    }), s.requestValidator && await s.requestValidator(s), s.body !== void 0 && s.bodySerializer && (s.serializedBody = s.bodySerializer(s.body)), (s.body === void 0 || s.serializedBody === "") && s.headers.delete("Content-Type");
    const f = P(s);
    return { opts: s, url: f };
  }, o = async (c) => {
    const { opts: s, url: f } = await l(c), C = {
      redirect: "follow",
      ...s,
      body: de(s)
    };
    let x = new Request(f, C);
    for (const h of a.request.fns)
      h && (x = await h(x, s));
    const E = s.fetch;
    let u = await E(x);
    for (const h of a.response.fns)
      h && (u = await h(u, x, s));
    const m = {
      request: x,
      response: u
    };
    if (u.ok) {
      const h = (s.parseAs === "auto" ? ue(u.headers.get("Content-Type")) : s.parseAs) ?? "json";
      if (u.status === 204 || u.headers.get("Content-Length") === "0") {
        let S;
        switch (h) {
          case "arrayBuffer":
          case "blob":
          case "text":
            S = await u[h]();
            break;
          case "formData":
            S = new FormData();
            break;
          case "stream":
            S = u.body;
            break;
          default:
            S = {};
            break;
        }
        return s.responseStyle === "data" ? S : {
          data: S,
          ...m
        };
      }
      let b;
      switch (h) {
        case "arrayBuffer":
        case "blob":
        case "formData":
        case "json":
        case "text":
          b = await u[h]();
          break;
        case "stream":
          return s.responseStyle === "data" ? u.body : {
            data: u.body,
            ...m
          };
      }
      return h === "json" && (s.responseValidator && await s.responseValidator(b), s.responseTransformer && (b = await s.responseTransformer(b))), s.responseStyle === "data" ? b : {
        data: b,
        ...m
      };
    }
    const v = await u.text();
    let z;
    try {
      z = JSON.parse(v);
    } catch {
    }
    const _ = z ?? v;
    let g = _;
    for (const h of a.error.fns)
      h && (g = await h(_, u, x, s));
    if (g = g || {}, s.throwOnError)
      throw g;
    return s.responseStyle === "data" ? void 0 : {
      error: g,
      ...m
    };
  }, n = (c) => (s) => o({ ...s, method: c }), d = (c) => async (s) => {
    const { opts: f, url: C } = await l(s);
    return te({
      ...f,
      body: f.body,
      headers: f.headers,
      method: c,
      onRequest: async (x, E) => {
        let u = new Request(x, E);
        for (const m of a.request.fns)
          m && (u = await m(u, f));
        return u;
      },
      url: C
    });
  };
  return {
    buildUrl: P,
    connect: n("CONNECT"),
    delete: n("DELETE"),
    get: n("GET"),
    getConfig: r,
    head: n("HEAD"),
    interceptors: a,
    options: n("OPTIONS"),
    patch: n("PATCH"),
    post: n("POST"),
    put: n("PUT"),
    request: o,
    setConfig: i,
    sse: {
      connect: d("CONNECT"),
      delete: d("DELETE"),
      get: d("GET"),
      head: d("HEAD"),
      options: d("OPTIONS"),
      patch: d("PATCH"),
      post: d("POST"),
      put: d("PUT"),
      trace: d("TRACE")
    },
    trace: n("TRACE")
  };
}, ge = (e) => ({
  ...e,
  ...Z.getConfig()
}), Se = ye(ge(F({
  baseUrl: "https://localhost:44341"
})));
class xe {
  static getConfiguration(t) {
    return (t?.client ?? Se).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/management/api/v1/charlietangoumbracoxmlsitemap/api/configuration",
      ...t
    });
  }
}
var ve = Object.defineProperty, _e = Object.getOwnPropertyDescriptor, I = (e, t, r, i) => {
  for (var a = i > 1 ? void 0 : i ? _e(t, r) : t, l = e.length - 1, o; l >= 0; l--)
    (o = e[l]) && (a = (i ? o(t, r, a) : o(a)) || a);
  return i && a && ve(t, r, a), a;
};
let w = class extends G {
  constructor() {
    super(...arguments), this._isLoading = !0;
  }
  connectedCallback() {
    super.connectedCallback(), this._loadConfiguration();
  }
  render() {
    if (this._isLoading)
      return p`<uui-box headline="XML Sitemaps"><uui-loader></uui-loader></uui-box>`;
    if (this._error)
      return p`
        <uui-box headline="XML Sitemaps">
          <div class="state state-error">${this._error}</div>
        </uui-box>
      `;
    const e = this._configuration;
    return e ? p`
      <uui-box headline="XML Sitemaps">
        <div class="status-row">
          <span class=${e.enabled ? "status enabled" : "status disabled"}>
            ${e.enabled ? "Enabled" : "Disabled"}
          </span>
          <span class="muted">Rewrites ${this._formatBoolean(e.rewritesEnabled)}</span>
          <span class="muted">
            Alternate links ${this._formatBoolean(e.renderAlternateLinksForSingleCultureSitemaps)}
          </span>
        </div>

        <dl class="summary-grid">
          ${this._renderSummaryItem("Configured sitemaps", e.sitemapCount)}
          ${this._renderSummaryItem("Custom sitemaps", e.customSitemapCount)}
          ${this._renderSummaryItem("Indexes", e.indexCount)}
          ${this._renderSummaryItem("Stale after", this._formatSeconds(e.storage?.refreshStaleAfterSeconds))}
          ${this._renderSummaryItem("Background job", this._formatBoolean(e.storage?.backgroundJobEnabled))}
          ${this._renderSummaryItem("Job interval", this._formatSeconds(e.storage?.backgroundJobIntervalSeconds))}
        </dl>
      </uui-box>

      <uui-box headline="Global filters">
        <div class="detail-grid">
          ${this._renderListDetail("Included cultures", e.globalFilters?.includedCultures)}
          ${this._renderListDetail("Excluded cultures", e.globalFilters?.excludedCultures)}
          ${this._renderListDetail("Included content types", e.globalFilters?.includedContentTypeAliases, "All")}
          ${this._renderListDetail("Excluded content types", e.globalFilters?.excludedContentTypeAliases)}
        </div>
      </uui-box>

      ${this._renderConfiguredSitemaps(e.sitemaps ?? [])}
      ${this._renderCustomSitemaps(e.customSitemaps ?? [])}
      ${this._renderIndexes(e.indexes ?? [])}
    ` : p`
        <uui-box headline="XML Sitemaps">
          <div class="state">No configuration was returned.</div>
        </uui-box>
      `;
  }
  async _loadConfiguration() {
    try {
      const e = await xe.getConfiguration({ throwOnError: !0 });
      this._configuration = e.data;
    } catch {
      this._error = "Unable to load sitemap configuration.";
    } finally {
      this._isLoading = !1;
    }
  }
  _renderConfiguredSitemaps(e) {
    return this._renderTable(
      "Configured Sitemaps",
      e,
      p`
        <tr>
          <th>Key</th>
          <th>Host</th>
          <th>Path</th>
          <th>Culture</th>
          <th>Included cultures</th>
          <th>Excluded cultures</th>
          <th>Included types</th>
          <th>Excluded types</th>
        </tr>
      `,
      (t) => p`
        <tr>
          <td>${this._formatValue(t.key)}</td>
          <td>${this._formatValue(t.hostName)}</td>
          <td>${this._formatValue(t.path)}</td>
          <td>${this._formatValue(t.culture)}</td>
          <td>${this._formatList(t.includedCultures)}</td>
          <td>${this._formatList(t.excludedCultures)}</td>
          <td>${this._formatList(t.includedDocumentTypeAliases)}</td>
          <td>${this._formatList(t.excludedDocumentTypeAliases)}</td>
        </tr>
      `,
      "No configured sitemaps."
    );
  }
  _renderCustomSitemaps(e) {
    return this._renderTable(
      "Custom Sitemaps",
      e,
      p`
        <tr>
          <th>Key</th>
          <th>Provider</th>
          <th>Host</th>
          <th>Settings</th>
        </tr>
      `,
      (t) => p`
        <tr>
          <td>${this._formatValue(t.key)}</td>
          <td>${this._formatValue(t.providerAlias)}</td>
          <td>${this._formatValue(t.hostName)}</td>
          <td>${this._formatSettingKeys(t.settingKeys, t.settingCount)}</td>
        </tr>
      `,
      "No custom sitemaps."
    );
  }
  _renderIndexes(e) {
    return this._renderTable(
      "Indexes",
      e,
      p`
        <tr>
          <th>Key</th>
          <th>Host</th>
          <th>Sitemaps</th>
        </tr>
      `,
      (t) => p`
        <tr>
          <td>${this._formatValue(t.key)}</td>
          <td>${this._formatValue(t.hostName)}</td>
          <td>${this._formatList(t.sitemaps)}</td>
        </tr>
      `,
      "No sitemap indexes."
    );
  }
  _renderTable(e, t, r, i, a) {
    return p`
      <uui-box headline=${e}>        

        ${t.length === 0 ? p`<div class="state">${a}</div>` : p`
              <div class="table-wrap">
                <table>
                  <thead>
                    ${r}
                  </thead>
                  <tbody>
                    ${t.map((l) => i(l))}
                  </tbody>
                </table>
              </div>
            `}
      </uui-box>
    `;
  }
  _renderSummaryItem(e, t) {
    return p`
      <div>
        <dt>${e}</dt>
        <dd>${this._formatValue(t)}</dd>
      </div>
    `;
  }
  _renderListDetail(e, t, r = "None") {
    return p`
      <div class="detail">
        <span>${e}</span>
        <strong>${this._formatList(t, r)}</strong>
      </div>
    `;
  }
  _formatSettingKeys(e, t) {
    const r = `${t ?? e?.length ?? 0} configured`;
    return e && e.length > 0 ? `${r}: ${e.join(", ")}` : r;
  }
  _formatSeconds(e) {
    return e == null ? "Not configured" : `${e} sec`;
  }
  _formatBoolean(e) {
    return e ? "enabled" : "disabled";
  }
  _formatList(e, t = "None") {
    return e && e.length > 0 ? e.join(", ") : t;
  }
  _formatValue(e) {
    return e == null || e === "" ? "Not configured" : e;
  }
};
w.styles = [
  Q`
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
    `
];
I([
  O()
], w.prototype, "_configuration", 2);
I([
  O()
], w.prototype, "_error", 2);
I([
  O()
], w.prototype, "_isLoading", 2);
w = I([
  Y("casko-xml-sitemaps-configuration-workspace-view")
], w);
const ze = w;
export {
  w as CaskoXmlSitemapsConfigurationWorkspaceViewElement,
  ze as default
};
//# sourceMappingURL=configuration-workspace-view.element-rQqOGFCX.js.map
