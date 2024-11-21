import { ArcRotateCamera } from "@babylonjs/core/Cameras/arcRotateCamera.js"
import { Engine } from "@babylonjs/core/Engines/engine.js"
import { HemisphericLight } from "@babylonjs/core/Lights/hemisphericLight.js"
import { MeshBuilder } from "@babylonjs/core/Meshes/meshBuilder.js"
import { Scene } from "@babylonjs/core/scene.js"
import { Vector3 } from "@babylonjs/core/Maths/math.vector.js"


export function StartEngine(view: HTMLCanvasElement) {

    const engine = new Engine(view, true)

    const scene = new Scene(engine)

    const camera = new ArcRotateCamera(
        "camera",
        Math.PI / 2,
        Math.PI / 3.2,
        2,
        Vector3.Zero(),
        scene)

    camera.attachControl(view)

    const light = new HemisphericLight(
        "light",
        new Vector3(0, 1, 0),
        scene)

    //const mesh = MeshBuilder.CreateGround("mesh", {}, scene)

    engine.runRenderLoop(() => {
        scene.render();
    })
}