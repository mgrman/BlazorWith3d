import "@babylonjs/core/Debug/debugLayer";
import "@babylonjs/inspector";
import {
    Engine,
    Scene,
    Vector3,
    Mesh,
    MeshBuilder,
    PointerDragBehavior,
    FreeCamera,
    Vector2,
    Tools,
    Matrix,
    DirectionalLight
} from "@babylonjs/core";
import {AddBlockInstance} from "com.blazorwith3d.exampleapp.client.shared/memorypack/AddBlockInstance";
import {
    BlazorControllerInitialized
} from "com.blazorwith3d.exampleapp.client.shared/memorypack/BlazorControllerInitialized";
import {PerfCheck} from "com.blazorwith3d.exampleapp.client.shared/memorypack/PerfCheck";
import {AddBlockTemplate} from "com.blazorwith3d.exampleapp.client.shared/memorypack/AddBlockTemplate";
import {RemoveBlockInstance} from "com.blazorwith3d.exampleapp.client.shared/memorypack/RemoveBlockInstance";
import {RemoveBlockTemplate} from "com.blazorwith3d.exampleapp.client.shared/memorypack/RemoveBlockTemplate";
import {StartDraggingBlock} from "com.blazorwith3d.exampleapp.client.shared/memorypack/StartDraggingBlock";
import {BlockPoseChangeValidated} from "com.blazorwith3d.exampleapp.client.shared/memorypack/BlockPoseChangeValidated";
import {UnityAppInitialized} from "com.blazorwith3d.exampleapp.client.shared/memorypack/UnityAppInitialized";
import {BlockPoseChanging} from "com.blazorwith3d.exampleapp.client.shared/memorypack/BlockPoseChanging";
import {BlockPoseChanged} from "com.blazorwith3d.exampleapp.client.shared/memorypack/BlockPoseChanged";
import { BlocksOnGridUnityApi } from "com.blazorwith3d.exampleapp.client.shared/memorypack/BlocksOnGridUnityApi";
import {BlazorBinaryApi} from "com.blazorwith3d.exampleapp.client.shared/BlazorBinaryApi";

export function InitializeBabylonApp(canvas: HTMLCanvasElement, dotnetObject: any, onMessageReceivedMethodName: string) {

    var sendMessageCallback: (msgBytes: Uint8Array) => Promise<any> = msgBytes => dotnetObject.invokeMethodAsync(onMessageReceivedMethodName, msgBytes);

    return new DebugApp(canvas, sendMessageCallback);
}


export class DebugApp {
    private _sendMessage: (msgBytes: Uint8Array) => Promise<any>;
    private scene: Scene;
    private templates: Array<AddBlockTemplate> = new Array<AddBlockTemplate>();
    private instances: Array<[instance: AddBlockInstance, mesh: Mesh, drag: PointerDragBehavior]> = new Array<[instance: AddBlockInstance, mesh: Mesh, drag: PointerDragBehavior]>();

    private changingRequestId: number = 0;
    private plane: Mesh;
    private _blazorApp: BlocksOnGridUnityApi;
    private _binaryApi: BlazorBinaryApi;

    constructor(canvas: HTMLCanvasElement, sendMessage: (msgBytes: Uint8Array) => Promise<any>) {

        // initialize babylon scene and engine
        var engine = new Engine(canvas, true);
        this.scene = new Scene(engine);

        var camera = new FreeCamera("Camera", new Vector3(0, 0, 10), this.scene);
        camera.setTarget(Vector3.Zero());
        var light1 = new DirectionalLight("light1", new Vector3(0, 3, 0), this.scene);
        light1.direction = new Vector3(-1.144162, -2.727118, -1.981747)

       this._binaryApi= new BlazorBinaryApi(sendMessage);
        this._blazorApp=new BlocksOnGridUnityApi(this._binaryApi);

        this.plane = MeshBuilder.CreatePlane("plane", {
            width: 10,
            height: 10,
            sideOrientation: Mesh.DOUBLESIDE

        }, this.scene);
        this.plane.rotation = new Vector3(Tools.ToRadians(4.92), Tools.ToRadians(159.77 - 180), Tools.ToRadians(180));


        // hide/show the Inspector
        window.addEventListener("keydown", (ev) => {
            // Shift+Ctrl+Alt+I
            if (ev.shiftKey && ev.ctrlKey && ev.altKey && ev.keyCode === 73) {
                if (this.scene.debugLayer.isVisible()) {
                    this.scene.debugLayer.hide();
                } else {
                    this.scene.debugLayer.show();
                }
            }
        });

        // run the main render loop
        engine.runRenderLoop(() => {
            this.scene.render();
        });
        
        this._blazorApp.OnBlazorControllerInitialized=msg=>this.OnBlazorControllerInitialized(msg);
        this._blazorApp.OnPerfCheck=msg=>this.OnPerfCheck(msg);
        this._blazorApp.OnAddBlockTemplate=msg=>this.OnAddBlockTemplate(msg);
        this._blazorApp.OnAddBlockInstance=msg=>this.OnAddBlockInstance(msg);
        this._blazorApp.OnRemoveBlockInstance=msg=>this.OnRemoveBlockInstance(msg);
        this._blazorApp.OnRemoveBlockTemplate=msg=>this.OnRemoveBlockTemplate(msg);
        this._blazorApp.OnStartDraggingBlock=msg=>this.OnStartDraggingBlock(msg);
        this._blazorApp.OnBlockPoseChangeValidated=msg=>this.OnBlockPoseChangeValidated(msg);

        this._blazorApp.StartProcessingMessages();

        this._blazorApp.InvokeUnityAppInitialized(new UnityAppInitialized()).then(_ => console.log("UnityAppInitialized invoked"));
    }

    public ProcessMessage(msg: Uint8Array): void {
        this._binaryApi.onMessageReceived(msg);
    }

    public Quit(): void {
        console.log("Quit called");
    }

    protected OnBlockPoseChangeValidated(obj: BlockPoseChangeValidated) {
        console.log("BlockPoseChangeValidated", obj);


        const [instance, mesh, drag] = this.instances.find(o => o[0].blockId === obj.blockId);

        instance.positionX = obj.newPositionX;
        instance.positionY = obj.newPositionY;
        instance.rotationZ = obj.newRotationZ;
        this.UpdateMeshPosition(mesh, instance);
    }

    protected OnStartDraggingBlock(obj: StartDraggingBlock) {
        console.log("StartDraggingBlock IS NOT SUPPORTED YET", obj);
    }

    protected OnRemoveBlockTemplate(obj: RemoveBlockTemplate) {
        console.log("RemoveBlockTemplate", obj);
        this.templates = this.templates.filter(o => o.templateId !== obj.templateId);
    }

    protected OnRemoveBlockInstance(obj: RemoveBlockInstance) {
        console.log("RemoveBlockInstance", obj);


        const [instance, mesh, drag] = this.instances.find(o => o[0].blockId === obj.blockId);
        this.instances = this.instances.filter(o => o[0].blockId !== obj.blockId);

        this.scene.removeMesh(mesh);
    }

    protected OnAddBlockInstance(obj: AddBlockInstance) {
        console.log("AddBlockInstance", obj);

        var template = this.templates.find(o => o.templateId === obj.templateId);

        var mesh: Mesh = MeshBuilder.CreateBox("box" + obj.blockId, {
            width: template.sizeX,
            height: template.sizeY,
            depth: template.sizeZ
        }, this.scene);
        mesh.parent = this.plane;
        mesh.position = new Vector3(0, 0, template.sizeZ / 2);
        this.UpdateMeshPosition(mesh, obj);

        var pointerDragBehavior = new PointerDragBehavior({dragPlaneNormal: new Vector3(0, 0, 1)});
        pointerDragBehavior.useObjectOrientationForDragging = false;
        pointerDragBehavior.updateDragPlane = false;

        var initialPoint: Vector2;


        const matrix = Matrix.Invert(this.plane.computeWorldMatrix(true));

        pointerDragBehavior.onDragStartObservable.add((event) => {
            console.log("dragStart");
            console.log(event);

            var localPoint = Vector3.TransformCoordinates(event.dragPlanePoint, matrix);

            initialPoint = new Vector2(localPoint.x - obj.positionX, localPoint.y - obj.positionY);
        });
        pointerDragBehavior.onDragObservable.add((event) => {
            console.log("drag");
            console.log(event);

            var localPoint = Vector3.TransformCoordinates(event.dragPlanePoint, matrix);

            this._blazorApp.InvokeBlockPoseChanging({

                blockId: obj.blockId,
                changingRequestId: this.changingRequestId++,
                positionX: localPoint.x - initialPoint.x,
                positionY: localPoint.y - initialPoint.y,
                rotationZ: obj.rotationZ
            }).then();
        });
        pointerDragBehavior.onDragEndObservable.add((event) => {
            console.log("dragEnd");
            console.log(event);
            this._blazorApp.InvokeBlockPoseChanged({
                blockId: obj.blockId,
                positionX: obj.positionX,
                positionY: obj.positionY,
                rotationZ: obj.rotationZ
            });
        });
        pointerDragBehavior.moveAttached = false;
        pointerDragBehavior.attach(mesh);
        this.instances.push([obj, mesh, pointerDragBehavior]);
    }

    private UpdateMeshPosition = (mesh: Mesh, obj: AddBlockInstance) => {

        mesh.position = new Vector3(obj.positionX, obj.positionY, mesh.position.z);
        mesh.rotation = new Vector3(0, 0, obj.rotationZ);
    }

    protected OnAddBlockTemplate(obj: AddBlockTemplate) {
        console.log("AddBlockTemplate", obj);
        this.templates.push(obj);
    }

    protected OnPerfCheck(obj: PerfCheck) {
        this._blazorApp.InvokePerfCheck(
            {
                aaa: obj.aaa,
                bbb: obj.bbb,
                ccc: obj.ccc,
                ddd: obj.ddd,
                id: obj.id
            }).then();
    }

    protected OnBlazorControllerInitialized(obj: BlazorControllerInitialized) {

        console.log("OnBlazorControllerInitialized", obj);

        this.templates = new Array<AddBlockTemplate>();


        for (const i of this.instances) {
            const [instance, mesh, drag] = i;

            this.scene.removeMesh(mesh);
        }

        this.instances = new Array<[instance: AddBlockInstance, mesh: Mesh, drag: PointerDragBehavior]>();

    }

}
