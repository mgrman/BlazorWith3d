import "@babylonjs/core/Debug/debugLayer";
import "@babylonjs/inspector";
import { Engine, Scene, ArcRotateCamera, Vector3, HemisphericLight, MeshBuilder } from "@babylonjs/core";
export function InitializeBabylonApp(canvas) {
    return new DebugApp(canvas);
}
export class DebugApp {
    constructor(canvas) {
        var engine = new Engine(canvas, true);
        var scene = new Scene(engine);
        var camera = new ArcRotateCamera("Camera", Math.PI / 2, Math.PI / 2, 2, Vector3.Zero(), scene);
        camera.attachControl(canvas, true);
        var light1 = new HemisphericLight("light1", new Vector3(1, 1, 0), scene);
        var sphere = MeshBuilder.CreateSphere("sphere", { diameter: 1 }, scene);
        window.addEventListener("keydown", (ev) => {
            if (ev.shiftKey && ev.ctrlKey && ev.altKey && ev.keyCode === 73) {
                if (scene.debugLayer.isVisible()) {
                    scene.debugLayer.hide();
                }
                else {
                    scene.debugLayer.show();
                }
            }
        });
        engine.runRenderLoop(() => {
            scene.render();
        });
    }
}
