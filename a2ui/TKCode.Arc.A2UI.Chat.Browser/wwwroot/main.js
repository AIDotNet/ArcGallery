import { dotnet } from './_framework/dotnet.js'

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .create();

// Settings and conversations live in this browser's local storage (BrowserChatStorage in Program.cs). Writes fail quietly
// when storage is full or blocked; the app then keeps them in memory for this visit.
dotnetRuntime.setModuleImports('a2ui-store', {
    languages: () => (navigator.languages?.length ? navigator.languages : [navigator.language ?? '']).join(','),
    get: key => {
        try {
            return localStorage.getItem(key) ?? '';
        } catch {
            return '';
        }
    },
    set: (key, value) => {
        try {
            localStorage.setItem(key, value);
            return true;
        } catch {
            return false;
        }
    },
    remove: key => {
        try {
            localStorage.removeItem(key);
        } catch {
        }
    },
    keys: prefix => {
        try {
            const keys = [];
            for (let i = 0; i < localStorage.length; i++) {
                const key = localStorage.key(i);
                if (key && key.startsWith(prefix)) {
                    keys.push(key);
                }
            }
            return keys.join('\n');
        } catch {
            return '';
        }
    }
});

// Files of the site as bytes (CjkFont.cs): file() is synchronous because the text layout asks for a glyph while it measures;
// fetchFile() gets a file ahead without blocking and takeFile() hands its bytes over once. Empty when there is no such file
// (a host that answers a missing file with index.html returns text that starts with '<').
const fetched = new Map();
dotnetRuntime.setModuleImports('a2ui-files', {
    file: path => {
        try {
            const request = new XMLHttpRequest();
            request.open('GET', path, false);
            request.overrideMimeType('text/plain; charset=x-user-defined');
            request.send();
            const text = request.status === 200 ? request.responseText : '';
            if (text.startsWith('<')) {
                return new Uint8Array(0);
            }
            const bytes = new Uint8Array(text.length);
            for (let i = 0; i < text.length; i++) {
                bytes[i] = text.charCodeAt(i) & 0xff;
            }
            return bytes;
        } catch {
            return new Uint8Array(0);
        }
    },
    fetchFile: async path => {
        try {
            const response = await fetch(path);
            if (response.ok && !/^text\/html\b/.test(response.headers.get('content-type') ?? '')) {
                fetched.set(path, new Uint8Array(await response.arrayBuffer()));
            }
        } catch {
        }
    },
    takeFile: path => {
        const bytes = fetched.get(path) ?? new Uint8Array(0);
        fetched.delete(path);
        return bytes;
    }
});

await dotnetRuntime.runMain();
