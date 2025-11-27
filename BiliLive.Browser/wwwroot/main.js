import { dotnet } from './_framework/dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .withResourceLoader((type, name, defaultUri, integrity) => {
        // dotnetjs 只能是 URL
        if (type === "dotnetjs") {
            return defaultUri;
        }

        // 其他资源才拦截，用于显示进度
        return fetch(defaultUri, { integrity }).then(resp => {
            const contentLength = resp.headers.get("Content-Length");
            if (contentLength) {
                totalBytes += parseInt(contentLength);
            }
            const reader = resp.body.getReader();

            return new Response(
                new ReadableStream({
                    async pull(controller) {
                        const { done, value } = await reader.read();
                        if (done) {
                            controller.close();
                            return;
                        }
                        loadedBytes += value.length;
                        const percent = totalBytes ? (loadedBytes / totalBytes) * 100 : 0;
                        text.textContent = `Loading... ${percent.toFixed(1)}%`;
                        controller.enqueue(value);
                    }
                })
            );
        });
    })
    .create();

const config = dotnetRuntime.getConfig();

await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);
