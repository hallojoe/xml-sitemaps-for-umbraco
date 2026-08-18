import { UmbLitElement as G } from "@umbraco-cms/backoffice/lit-element";
import { html as f, css as Q, state as L, customElement as Y } from "@umbraco-cms/backoffice/external/lit";
import { umbHttpClient as Z } from "@umbraco-cms/backoffice/http-client";
const ee = {
  bodySerializer: (e) => JSON.stringify(
    e,
    (r, t) => typeof t == "bigint" ? t.toString() : t
  )
}, te = ({
  onRequest: e,
  onSseError: r,
  onSseEvent: t,
  responseTransformer: s,
  responseValidator: a,
  sseDefaultRetryDelay: d,
  sseMaxRetryAttempts: o,
  sseMaxRetryDelay: n,
  sseSleepFn: l,
  url: c,
  ...i
}) => {
  let h;
  const C = l ?? ((u) => new Promise((m) => setTimeout(m, u)));
  return { stream: async function* () {
    let u = d ?? 3e3, m = 0;
    const $ = i.signal ?? new AbortController().signal;
    for (; !$.aborted; ) {
      m++;
      const z = i.headers instanceof Headers ? i.headers : new Headers(i.headers);
      h !== void 0 && z.set("Last-Event-ID", h);
      try {
        const x = {
          redirect: "follow",
          ...i,
          body: i.serializedBody,
          headers: z,
          signal: $
        };
        let y = new Request(c, x);
        e && (y = await e(c, x));
        const b = await (i.fetch ?? globalThis.fetch)(y);
        if (!b.ok)
          throw new Error(
            `SSE failed: ${b.status} ${b.statusText}`
          );
        if (!b.body) throw new Error("No body in SSE response");
        const S = b.body.pipeThrough(new TextDecoderStream()).getReader();
        let N = "";
        const T = () => {
          try {
            S.cancel();
          } catch {
          }
        };
        $.addEventListener("abort", T);
        try {
          for (; ; ) {
            const { done: J, value: K } = await S.read();
            if (J) break;
            N += K;
            const P = N.split(`

`);
            N = P.pop() ?? "";
            for (const M of P) {
              const X = M.split(`
`), k = [];
              let O;
              for (const g of X)
                if (g.startsWith("data:"))
                  k.push(g.replace(/^data:\s*/, ""));
                else if (g.startsWith("event:"))
                  O = g.replace(/^event:\s*/, "");
                else if (g.startsWith("id:"))
                  h = g.replace(/^id:\s*/, "");
                else if (g.startsWith("retry:")) {
                  const q = Number.parseInt(
                    g.replace(/^retry:\s*/, ""),
                    10
                  );
                  Number.isNaN(q) || (u = q);
                }
              let v, D = !1;
              if (k.length) {
                const g = k.join(`
`);
                try {
                  v = JSON.parse(g), D = !0;
                } catch {
                  v = g;
                }
              }
              D && (a && await a(v), s && (v = await s(v))), t?.({
                data: v,
                event: O,
                id: h,
                retry: u
              }), k.length && (yield v);
            }
          }
        } finally {
          $.removeEventListener("abort", T), S.releaseLock();
        }
        break;
      } catch (x) {
        if (r?.(x), o !== void 0 && m >= o)
          break;
        const y = Math.min(
          u * 2 ** (m - 1),
          n ?? 3e4
        );
        await C(y);
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
}, B = ({
  allowReserved: e,
  explode: r,
  name: t,
  style: s,
  value: a
}) => {
  if (!r) {
    const n = (e ? a : a.map((l) => encodeURIComponent(l))).join(ae(s));
    switch (s) {
      case "label":
        return `.${n}`;
      case "matrix":
        return `;${t}=${n}`;
      case "simple":
        return n;
      default:
        return `${t}=${n}`;
    }
  }
  const d = re(s), o = a.map((n) => s === "label" || s === "simple" ? e ? n : encodeURIComponent(n) : j({
    allowReserved: e,
    name: t,
    value: n
  })).join(d);
  return s === "label" || s === "matrix" ? d + o : o;
}, j = ({
  allowReserved: e,
  name: r,
  value: t
}) => {
  if (t == null)
    return "";
  if (typeof t == "object")
    throw new Error(
      "Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these."
    );
  return `${r}=${e ? t : encodeURIComponent(t)}`;
}, R = ({
  allowReserved: e,
  explode: r,
  name: t,
  style: s,
  value: a,
  valueOnly: d
}) => {
  if (a instanceof Date)
    return d ? a.toISOString() : `${t}=${a.toISOString()}`;
  if (s !== "deepObject" && !r) {
    let l = [];
    Object.entries(a).forEach(([i, h]) => {
      l = [
        ...l,
        i,
        e ? h : encodeURIComponent(h)
      ];
    });
    const c = l.join(",");
    switch (s) {
      case "form":
        return `${t}=${c}`;
      case "label":
        return `.${c}`;
      case "matrix":
        return `;${t}=${c}`;
      default:
        return c;
    }
  }
  const o = se(s), n = Object.entries(a).map(
    ([l, c]) => j({
      allowReserved: e,
      name: s === "deepObject" ? `${t}[${l}]` : l,
      value: c
    })
  ).join(o);
  return s === "label" || s === "matrix" ? o + n : n;
}, ie = /\{[^{}]+\}/g, ne = ({ path: e, url: r }) => {
  let t = r;
  const s = r.match(ie);
  if (s)
    for (const a of s) {
      let d = !1, o = a.substring(1, a.length - 1), n = "simple";
      o.endsWith("*") && (d = !0, o = o.substring(0, o.length - 1)), o.startsWith(".") ? (o = o.substring(1), n = "label") : o.startsWith(";") && (o = o.substring(1), n = "matrix");
      const l = e[o];
      if (l == null)
        continue;
      if (Array.isArray(l)) {
        t = t.replace(
          a,
          B({ explode: d, name: o, style: n, value: l })
        );
        continue;
      }
      if (typeof l == "object") {
        t = t.replace(
          a,
          R({
            explode: d,
            name: o,
            style: n,
            value: l,
            valueOnly: !0
          })
        );
        continue;
      }
      if (n === "matrix") {
        t = t.replace(
          a,
          `;${j({
            name: o,
            value: l
          })}`
        );
        continue;
      }
      const c = encodeURIComponent(
        n === "label" ? `.${l}` : l
      );
      t = t.replace(a, c);
    }
  return t;
}, oe = ({
  baseUrl: e,
  path: r,
  query: t,
  querySerializer: s,
  url: a
}) => {
  const d = a.startsWith("/") ? a : `/${a}`;
  let o = (e ?? "") + d;
  r && (o = ne({ path: r, url: o }));
  let n = t ? s(t) : "";
  return n.startsWith("?") && (n = n.substring(1)), n && (o += `?${n}`), o;
};
function le(e) {
  const r = e.body !== void 0;
  if (r && e.bodySerializer)
    return "serializedBody" in e ? e.serializedBody !== void 0 && e.serializedBody !== "" ? e.serializedBody : null : e.body !== "" ? e.body : null;
  if (r)
    return e.body;
}
const de = async (e, r) => {
  const t = typeof r == "function" ? await r(e) : r;
  if (t)
    return e.scheme === "bearer" ? `Bearer ${t}` : e.scheme === "basic" ? `Basic ${btoa(t)}` : t;
}, H = ({
  allowReserved: e,
  array: r,
  object: t
} = {}) => (a) => {
  const d = [];
  if (a && typeof a == "object")
    for (const o in a) {
      const n = a[o];
      if (n != null)
        if (Array.isArray(n)) {
          const l = B({
            allowReserved: e,
            explode: !0,
            name: o,
            style: "form",
            value: n,
            ...r
          });
          l && d.push(l);
        } else if (typeof n == "object") {
          const l = R({
            allowReserved: e,
            explode: !0,
            name: o,
            style: "deepObject",
            value: n,
            ...t
          });
          l && d.push(l);
        } else {
          const l = j({
            allowReserved: e,
            name: o,
            value: n
          });
          l && d.push(l);
        }
    }
  return d.join("&");
}, ue = (e) => {
  if (!e)
    return "stream";
  const r = e.split(";")[0]?.trim();
  if (r) {
    if (r.startsWith("application/json") || r.endsWith("+json"))
      return "json";
    if (r === "multipart/form-data")
      return "formData";
    if (["application/", "audio/", "image/", "video/"].some(
      (t) => r.startsWith(t)
    ))
      return "blob";
    if (r.startsWith("text/"))
      return "text";
  }
}, ce = (e, r) => r ? !!(e.headers.has(r) || e.query?.[r] || e.headers.get("Cookie")?.includes(`${r}=`)) : !1, fe = async ({
  security: e,
  ...r
}) => {
  for (const t of e) {
    if (ce(r, t.name))
      continue;
    const s = await de(t, r.auth);
    if (!s)
      continue;
    const a = t.name ?? "Authorization";
    switch (t.in) {
      case "query":
        r.query || (r.query = {}), r.query[a] = s;
        break;
      case "cookie":
        r.headers.append("Cookie", `${a}=${s}`);
        break;
      default:
        r.headers.set(a, s);
        break;
    }
  }
}, V = (e) => oe({
  baseUrl: e.baseUrl,
  path: e.path,
  query: e.query,
  querySerializer: typeof e.querySerializer == "function" ? e.querySerializer : H(e.querySerializer),
  url: e.url
}), U = (e, r) => {
  const t = { ...e, ...r };
  return t.baseUrl?.endsWith("/") && (t.baseUrl = t.baseUrl.substring(0, t.baseUrl.length - 1)), t.headers = W(e.headers, r.headers), t;
}, he = (e) => {
  const r = [];
  return e.forEach((t, s) => {
    r.push([s, t]);
  }), r;
}, W = (...e) => {
  const r = new Headers();
  for (const t of e) {
    if (!t)
      continue;
    const s = t instanceof Headers ? he(t) : Object.entries(t);
    for (const [a, d] of s)
      if (d === null)
        r.delete(a);
      else if (Array.isArray(d))
        for (const o of d)
          r.append(a, o);
      else d !== void 0 && r.set(
        a,
        typeof d == "object" ? JSON.stringify(d) : d
      );
  }
  return r;
};
class A {
  constructor() {
    this.fns = [];
  }
  clear() {
    this.fns = [];
  }
  eject(r) {
    const t = this.getInterceptorIndex(r);
    this.fns[t] && (this.fns[t] = null);
  }
  exists(r) {
    const t = this.getInterceptorIndex(r);
    return !!this.fns[t];
  }
  getInterceptorIndex(r) {
    return typeof r == "number" ? this.fns[r] ? r : -1 : this.fns.indexOf(r);
  }
  update(r, t) {
    const s = this.getInterceptorIndex(r);
    return this.fns[s] ? (this.fns[s] = t, r) : !1;
  }
  use(r) {
    return this.fns.push(r), this.fns.length - 1;
  }
}
const pe = () => ({
  error: new A(),
  request: new A(),
  response: new A()
}), me = H({
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
}), ge = (e = {}) => {
  let r = U(F(), e);
  const t = () => ({ ...r }), s = (c) => (r = U(r, c), t()), a = pe(), d = async (c) => {
    const i = {
      ...r,
      ...c,
      fetch: c.fetch ?? r.fetch ?? globalThis.fetch,
      headers: W(r.headers, c.headers),
      serializedBody: void 0
    };
    i.security && await fe({
      ...i,
      security: i.security
    }), i.requestValidator && await i.requestValidator(i), i.body !== void 0 && i.bodySerializer && (i.serializedBody = i.bodySerializer(i.body)), (i.body === void 0 || i.serializedBody === "") && i.headers.delete("Content-Type");
    const h = V(i);
    return { opts: i, url: h };
  }, o = async (c) => {
    const { opts: i, url: h } = await d(c), C = {
      redirect: "follow",
      ...i,
      body: le(i)
    };
    let _ = new Request(h, C);
    for (const p of a.request.fns)
      p && (_ = await p(_, i));
    const E = i.fetch;
    let u = await E(_);
    for (const p of a.response.fns)
      p && (u = await p(u, _, i));
    const m = {
      request: _,
      response: u
    };
    if (u.ok) {
      const p = (i.parseAs === "auto" ? ue(u.headers.get("Content-Type")) : i.parseAs) ?? "json";
      if (u.status === 204 || u.headers.get("Content-Length") === "0") {
        let S;
        switch (p) {
          case "arrayBuffer":
          case "blob":
          case "text":
            S = await u[p]();
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
        return i.responseStyle === "data" ? S : {
          data: S,
          ...m
        };
      }
      let b;
      switch (p) {
        case "arrayBuffer":
        case "blob":
        case "formData":
        case "json":
        case "text":
          b = await u[p]();
          break;
        case "stream":
          return i.responseStyle === "data" ? u.body : {
            data: u.body,
            ...m
          };
      }
      return p === "json" && (i.responseValidator && await i.responseValidator(b), i.responseTransformer && (b = await i.responseTransformer(b))), i.responseStyle === "data" ? b : {
        data: b,
        ...m
      };
    }
    const $ = await u.text();
    let z;
    try {
      z = JSON.parse($);
    } catch {
    }
    const x = z ?? $;
    let y = x;
    for (const p of a.error.fns)
      p && (y = await p(x, u, _, i));
    if (y = y || {}, i.throwOnError)
      throw y;
    return i.responseStyle === "data" ? void 0 : {
      error: y,
      ...m
    };
  }, n = (c) => (i) => o({ ...i, method: c }), l = (c) => async (i) => {
    const { opts: h, url: C } = await d(i);
    return te({
      ...h,
      body: h.body,
      headers: h.headers,
      method: c,
      onRequest: async (_, E) => {
        let u = new Request(_, E);
        for (const m of a.request.fns)
          m && (u = await m(u, h));
        return u;
      },
      url: C
    });
  };
  return {
    buildUrl: V,
    connect: n("CONNECT"),
    delete: n("DELETE"),
    get: n("GET"),
    getConfig: t,
    head: n("HEAD"),
    interceptors: a,
    options: n("OPTIONS"),
    patch: n("PATCH"),
    post: n("POST"),
    put: n("PUT"),
    request: o,
    setConfig: s,
    sse: {
      connect: l("CONNECT"),
      delete: l("DELETE"),
      get: l("GET"),
      head: l("HEAD"),
      options: l("OPTIONS"),
      patch: l("PATCH"),
      post: l("POST"),
      put: l("PUT"),
      trace: l("TRACE")
    },
    trace: n("TRACE")
  };
}, ye = (e) => ({
  ...e,
  ...Z.getConfig()
}), Se = ge(ye(F({
  baseUrl: "https://backoffice.dev.localhost"
})));
class _e {
  static getConfiguration(r) {
    return (r?.client ?? Se).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/xmlsitemapsforumbraco/api/v1/configuration",
      ...r
    });
  }
}
var $e = Object.defineProperty, xe = Object.getOwnPropertyDescriptor, I = (e, r, t, s) => {
  for (var a = s > 1 ? void 0 : s ? xe(r, t) : r, d = e.length - 1, o; d >= 0; d--)
    (o = e[d]) && (a = (s ? o(r, t, a) : o(a)) || a);
  return s && a && $e(r, t, a), a;
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
      return f`<uui-box headline="XML Sitemaps"><uui-loader></uui-loader></uui-box>`;
    if (this._error)
      return f`
        <uui-box headline="XML Sitemaps">
          <div class="state state-error">${this._error}</div>
        </uui-box>
      `;
    const e = this._configuration;
    return e ? f`
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
          ${this._renderSummaryItem("Root search level", e.rootNodeSearchLevel)}
          ${this._renderSummaryItem("Version cleanup", this._formatSeconds(e.storage?.versionCleanupAfterSeconds))}
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
          ${this._renderDetail("Excluding URL property", e.globalFilters?.excludingUrlPropertyAlias)}
          ${this._renderDetail("Excluding URL value", e.globalFilters?.excludingUrlPropertyValue)}
        </div>
      </uui-box>

      ${this._renderConfiguredSitemaps(e.sitemaps ?? [], e.rewritesEnabled)}
      ${this._renderCustomSitemaps(e.customSitemaps ?? [], e.rewritesEnabled)}
      ${this._renderIndexes(e.indexes ?? [], e.rewritesEnabled)}
    ` : f`
        <uui-box headline="XML Sitemaps">
          <div class="state">No configuration was returned.</div>
        </uui-box>
      `;
  }
  async _loadConfiguration() {
    try {
      const e = await _e.getConfiguration({ throwOnError: !0 });
      this._configuration = e.data;
    } catch {
      this._error = "Unable to load sitemap configuration.";
    } finally {
      this._isLoading = !1;
    }
  }
  _renderConfiguredSitemaps(e, r) {
    return this._renderTable(
      "Configured Sitemaps",
      e,
      f`
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
      (t) => f`
        <tr>
          <td>${this._formatValue(t.key)}</td>
          <td>${this._renderSitemapReference(this._getPublicName(t), t.hostName, r)}</td>
          <td>${this._formatValue(t.hostName)}</td>
          <td>${this._formatValue(t.path)}</td>
          <td>${this._formatValue(t.culture)}</td>
          <td>${this._formatList(t.includedCultures)}</td>
          <td>${this._formatList(t.excludedCultures)}</td>
          <td>${this._formatList(t.includedDocumentTypeAliases, "All")}</td>
          <td>${this._formatList(t.excludedDocumentTypeAliases)}</td>
        </tr>
      `,
      "No configured sitemaps."
    );
  }
  _renderCustomSitemaps(e, r) {
    return this._renderTable(
      "Custom Sitemaps",
      e,
      f`
        <tr>
          <th>Key</th>
          <th>Public name</th>
          <th>Provider</th>
          <th>Host</th>
          <th>Settings</th>
        </tr>
      `,
      (t) => f`
        <tr>
          <td>${this._formatValue(t.key)}</td>
          <td>${this._renderSitemapReference(this._getPublicName(t), t.hostName, r)}</td>
          <td>${this._formatValue(t.providerAlias)}</td>
          <td>${this._formatValue(t.hostName)}</td>
          <td>${this._formatSettingKeys(t.settingKeys, t.settingCount)}</td>
        </tr>
      `,
      "No custom sitemaps."
    );
  }
  _renderIndexes(e, r) {
    return this._renderTable(
      "Indexes",
      e,
      f`
        <tr>
          <th>Key</th>
          <th>Public name</th>
          <th>Host</th>
          <th>Sitemaps</th>
        </tr>
      `,
      (t) => f`
        <tr>
          <td>${this._formatValue(t.key)}</td>
          <td>${this._renderSitemapReference(this._getPublicName(t), t.hostName, r)}</td>
          <td>${this._formatValue(t.hostName)}</td>
          <td>${this._renderSitemapReferenceList(this._getPublicSitemaps(t), t.hostName, r)}</td>
        </tr>
      `,
      "No sitemap indexes."
    );
  }
  _renderTable(e, r, t, s, a) {
    return f`
      <uui-box headline=${e}>        

        ${r.length === 0 ? f`<div class="state">${a}</div>` : f`
              <div class="table-wrap">
                <table>
                  <thead>
                    ${t}
                  </thead>
                  <tbody>
                    ${r.map((d) => s(d))}
                  </tbody>
                </table>
              </div>
            `}
      </uui-box>
    `;
  }
  _renderSummaryItem(e, r) {
    return f`
      <div>
        <dt>${e}</dt>
        <dd>${this._formatValue(r)}</dd>
      </div>
    `;
  }
  _renderListDetail(e, r, t = "None") {
    return f`
      <div class="detail">
        <span>${e}</span>
        <strong>${this._formatList(r, t)}</strong>
      </div>
    `;
  }
  _renderDetail(e, r) {
    return f`
      <div class="detail">
        <span>${e}</span>
        <strong>${this._formatValue(r)}</strong>
      </div>
    `;
  }
  _renderSitemapReference(e, r, t) {
    return !t || !e ? this._formatValue(e) : f`
      <a class="sitemap-link" href=${this._buildSitemapUrl(e, r)} target="_blank" rel="noopener noreferrer">
        ${e}
      </a>
    `;
  }
  _renderSitemapReferenceList(e, r, t) {
    return !e || e.length === 0 ? "None" : t ? f`
      <span class="sitemap-link-list">
        ${e.map(
      (s, a) => f`
            ${a > 0 ? f`<span>, </span>` : void 0}
            ${this._renderSitemapReference(s, r, t)}
          `
    )}
      </span>
    ` : this._formatList(e);
  }
  _getPublicName(e) {
    return e.publicName || e.key;
  }
  _getPublicSitemaps(e) {
    const r = e;
    return r.publicSitemaps && r.publicSitemaps.length > 0 ? r.publicSitemaps : e.sitemaps;
  }
  _buildSitemapUrl(e, r) {
    const t = `/${encodeURIComponent(e)}.xml`, s = r?.trim();
    if (!s)
      return t;
    const a = s.replace(/\/+$/, "");
    return /^https?:\/\//i.test(a) ? `${a}${t}` : `https://${a}${t}`;
  }
  _formatSettingKeys(e, r) {
    const t = `${r ?? e?.length ?? 0} configured`;
    return e && e.length > 0 ? `${t}: ${e.join(", ")}` : t;
  }
  _formatSeconds(e) {
    return e == null ? "Not configured" : `${e} sec`;
  }
  _formatBoolean(e) {
    return e ? "enabled" : "disabled";
  }
  _formatList(e, r = "None") {
    return e && e.length > 0 ? e.join(", ") : r;
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
    `
];
I([
  L()
], w.prototype, "_configuration", 2);
I([
  L()
], w.prototype, "_error", 2);
I([
  L()
], w.prototype, "_isLoading", 2);
w = I([
  Y("casko-xml-sitemaps-configuration-workspace-view")
], w);
const ze = w;
export {
  w as CaskoXmlSitemapsConfigurationWorkspaceViewElement,
  ze as default
};
//# sourceMappingURL=configuration-workspace-view.element-CyGStWCU.js.map
