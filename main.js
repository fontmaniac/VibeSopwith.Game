import { dotnet } from './_framework/dotnet.js';

// Process content manifest
let contentManifest = await globalThis.fetch("content.txt");
let contentManifestText = "";
if (!contentManifest.ok) {
    console.error("Unable to load content manifest");
    console.error(contentManifest);
}
else {
    contentManifestText = await contentManifest.text();
}
let assetList = 
    contentManifestText
        .split('\n')
        .filter(i => i)
        .map(i => i.trim().replaceAll('\\', '/'));

console.log(`Found ${assetList.length} assets in manifest`);
console.log(assetList);

// Process dotnet config
const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false) // turn this on for now
    .withModuleConfig({
        onConfigLoaded: (config) => {
            config.resources = config.resources || {};
            config.resources.vfs = config.resources.vfs || {};

            console.log("VFS before:", config.resources.vfs);

            for (let asset of assetList) {
                asset = asset.trim();
                if (asset[0] === '/') {
                    asset = asset.substring(1);
                }
                console.log(`Found ${asset}, adding to VFS`);
                config.resources.vfs[asset] = {};
                const assetPath = `../${asset}`;
                config.resources.vfs[asset][assetPath] = null;
            }

            console.log("VFS after:", config.resources.vfs);            
        },
    })
    .withApplicationArgumentsFromQuery()
    .create();

const canvas = document.getElementById("canvas");
dotnet.instance.Module.canvas = canvas;

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);
const onUserInteraction = exports.Program.OnUserInteraction;

canvas.addEventListener("click", (e) => {
    onUserInteraction()
});
canvas.addEventListener("touchstart", (e) => {
    onUserInteraction()
});

setModuleImports('main.js', {
    setMainLoop: (cb) => dotnet.instance.Module.setMainLoop(cb)
});

globalThis.dotnetInstance = dotnet.instance;

try {
    await dotnet.run();
} catch (e) {
    console.error("dotnet.run() failed with:", e);
}
