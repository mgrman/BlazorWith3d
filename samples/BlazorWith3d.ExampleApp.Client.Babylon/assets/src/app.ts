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
    DirectionalLight,
    PhysicsRaycastResult,
    Ray,
    Quaternion
} from "@babylonjs/core";
import {AddBlockInstance} from "com.blazorwith3d.exampleapp.client.shared/memorypack/AddBlockInstance";
import {
    BlazorControllerInitialized
} from "com.blazorwith3d.exampleapp.client.shared/memorypack/BlazorControllerInitialized";
import {PerfCheck} from "com.blazorwith3d.exampleapp.client.shared/memorypack/PerfCheck";
import {AddBlockTemplate} from "com.blazorwith3d.exampleapp.client.shared/memorypack/AddBlockTemplate";
import {RemoveBlockInstance} from "com.blazorwith3d.exampleapp.client.shared/memorypack/RemoveBlockInstance";
import {RemoveBlockTemplate} from "com.blazorwith3d.exampleapp.client.shared/memorypack/RemoveBlockTemplate";
import { UnityAppInitialized } from "com.blazorwith3d.exampleapp.client.shared/memorypack/UnityAppInitialized";
import { UpdateBlockInstance } from "com.blazorwith3d.exampleapp.client.shared/memorypack/UpdateBlockInstance";
import {
    BlocksOnGridUnityApi_BinaryApi,
    BlocksOnGridUnityApi_MethodInvoker_DirectInterop,
    IBlocksOnGridUnityApi_EventHandler,
    IBlocksOnGridUnityApi_MethodInvoker
} from "com.blazorwith3d.exampleapp.client.shared/memorypack/IBlocksOnGridUnityApi";
import {BlazorBinaryApi} from "com.blazorwith3d.exampleapp.client.shared/BlazorBinaryApi";
import { RequestRaycast } from "com.blazorwith3d.exampleapp.client.shared/memorypack/RequestRaycast";
import { RequestScreenToWorldRay } from "com.blazorwith3d.exampleapp.client.shared/memorypack/RequestScreenToWorldRay";
import { ScreenToWorldRayResponse } from "com.blazorwith3d.exampleapp.client.shared/memorypack/ScreenToWorldRayResponse";
import { RaycastResponse } from "com.blazorwith3d.exampleapp.client.shared/memorypack/RaycastResponse";

export function InitializeApp_BinaryApi(canvas: HTMLCanvasElement, dotnetObject: any, onMessageReceivedMethodName: string) {
    let sendMessageCallback: (msgBytes: Uint8Array) => Promise<any> = msgBytes => dotnetObject.invokeMethodAsync(onMessageReceivedMethodName, msgBytes);


    let binaryApi= new BlazorBinaryApi(sendMessageCallback);
    let blazorApp=new BlocksOnGridUnityApi_BinaryApi(binaryApi);

    let app= new DebugApp(canvas, blazorApp);

    blazorApp.SetEventHandler(app);

    let appAsAny :any =app ;
    appAsAny.ProcessMessage= msg=> {
        return binaryApi.mainMessageHandler(msg);
    }
    return appAsAny;
}

export function InitializeApp_DirectInterop(canvas: HTMLCanvasElement, dotnetObject: any) {
    return new DebugApp(canvas, new BlocksOnGridUnityApi_MethodInvoker_DirectInterop(dotnetObject));
}

export class DebugApp implements IBlocksOnGridUnityApi_EventHandler{
    private templates: Array<AddBlockTemplate> = new Array<AddBlockTemplate>();
    private instances: Array<[instance: AddBlockInstance, mesh: Mesh]> = new Array<[instance: AddBlockInstance, mesh: Mesh]>();

    private canvas: HTMLCanvasElement;
    private scene: Scene;
    private plane: Mesh;
    private _methodInvoker: IBlocksOnGridUnityApi_MethodInvoker;

    constructor(canvas: HTMLCanvasElement, methodInvoker: IBlocksOnGridUnityApi_MethodInvoker) {

        this.canvas=canvas;
        this._methodInvoker = methodInvoker;
        canvas.style.width = "100%";
        canvas.style.height = "100%";
        canvas.width = canvas.offsetWidth;
        canvas.height = canvas.offsetHeight;


        // initialize babylon scene and engine
        var engine = new Engine(canvas, true);
        this.scene = new Scene(engine);
        this.scene.preventDefaultOnPointerDown = false;
        this.scene.preventDefaultOnPointerUp = false;


        this.plane = new Mesh("plane", this.scene);
        // plane handled the conversion to Blazor coordinate system
        this.plane.rotation = new Vector3(Tools.ToRadians(0), Tools.ToRadians(0), Tools.ToRadians(0));


        var camera = new FreeCamera("Camera", new Vector3(0, 0, -10), this.scene);
        // camera.rotation.x = Tools.ToRadians(0);
        // camera.rotation.y = Tools.ToRadians(180);
        // camera.rotation.y = Tools.ToRadians(90);
    
        camera.parent = this.plane;
        
        camera.target = new Vector3(0, 0, 0)
        camera.upVector = new Vector3(0, 1, 0)


        var light1 = new DirectionalLight("light1", new Vector3(0, 0, 1).applyRotationQuaternion(Quaternion.FromEulerAngles(Tools.ToRadians(53), Tools.ToRadians(-18), Tools.ToRadians(0))), this.scene);
        light1.parent = this.plane;


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

        this._methodInvoker.InvokeUnityAppInitialized(new UnityAppInitialized()).then(_ => console.log("UnityAppInitialized invoked"));
    }

    public Quit(): void {
        console.log("Quit called");
    }

    public async OnUpdateBlockInstance(obj: UpdateBlockInstance) : Promise<any> {
        console.log("OnUpdateBlockInstance", obj);


        const [instance, mesh] = this.instances.find(o => o[0].blockId === obj.blockId);

        instance.position = obj.position;
        instance.rotationZ = obj.rotationZ;
        this.UpdateMeshPosition(mesh, instance);
    }


    public async OnRemoveBlockTemplate(obj: RemoveBlockTemplate): Promise<any>  {
        console.log("RemoveBlockTemplate", obj);
        this.templates = this.templates.filter(o => o.templateId !== obj.templateId);
    }

    public async OnRemoveBlockInstance(obj: RemoveBlockInstance): Promise<any>  {
        console.log("RemoveBlockInstance", obj);

        const [instance, mesh] = this.instances.find(o => o[0].blockId === obj.blockId);
        this.instances = this.instances.filter(o => o[0].blockId !== obj.blockId);

        this.scene.removeMesh(mesh);
    }

    public async OnAddBlockInstance(obj: AddBlockInstance): Promise<any>  {
        console.log("AddBlockInstance", obj);

        var template = this.templates.find(o => o.templateId === obj.templateId);

        var mesh: Mesh = MeshBuilder.CreateBox("box" + obj.blockId, {
            width: template.size.x,
            height: template.size.y,
            depth: template.size.z
        }, this.scene);
        mesh.parent = this.plane;
        mesh.position = new Vector3(0, 0, -template.size.z / 2);
        this.UpdateMeshPosition(mesh, obj);

        this.instances.push([obj, mesh,]);
    }

    private UpdateMeshPosition = (mesh: Mesh, obj: AddBlockInstance) => {

        mesh.position = new Vector3(obj.position.x, obj.position.y, mesh.position.z);
        mesh.rotation = new Vector3(0, 0, obj.rotationZ);
    }


    public async OnRequestScreenToWorldRay(msg: RequestScreenToWorldRay): Promise<ScreenToWorldRayResponse> {


        var ray = this.scene.createPickingRay(msg.screen.x, msg.screen.y, Matrix.Identity(), this.scene.activeCamera);	

        var worldToBlazor = Matrix.Invert(this.plane.computeWorldMatrix(true));

        // convert ray to expected blazor world coordinate system
        ray = new Ray(Vector3.TransformCoordinates(ray.origin, worldToBlazor), Vector3.TransformNormal(ray.direction, worldToBlazor), ray.length, ray.epsilon);



        return {
            ray: {
                origin: { x: ray.origin.x, y: ray.origin.y, z: ray.origin.z },
                direction: { x: ray.direction.x, y: ray.direction.y, z: ray.direction.z }
            }
        };
    }

    public async OnRequestRaycast(msg: RequestRaycast): Promise<RaycastResponse> {

        var start = new Vector3(msg.ray.origin.x, msg.ray.origin.y, msg.ray.origin.z);
        var dir = new Vector3(msg.ray.direction.x, msg.ray.direction.y, msg.ray.direction.z);

        var blazorToWorld = this.plane.computeWorldMatrix(true)
        var worldToBlazor = Matrix.Invert(this.plane.computeWorldMatrix(true));

        var ray = new Ray(Vector3.TransformCoordinates(start, blazorToWorld), Vector3.TransformNormal(dir, blazorToWorld));

        var pickingInfo = this.scene.pickWithRay(ray);

        var response = new RaycastResponse();
        response.hitWorld = start;

        if (pickingInfo.hit ) {

            
            const [instance, mesh] = this.instances.find(o => o[1] === pickingInfo.pickedMesh);
            response.hitBlockId = instance.blockId;
            response.hitWorld = Vector3.TransformCoordinates(pickingInfo.pickedPoint, worldToBlazor) ;
        }
         return response;
    }

    public async OnAddBlockTemplate(template: AddBlockTemplate):Promise<any>  {
        console.log("AddBlockTemplate", template);
        this.templates.push(template);
    }

    public async OnPerfCheck(obj: PerfCheck) :Promise<PerfCheck>
    {
        return  obj;
    }

    public async OnBlazorControllerInitialized(obj: BlazorControllerInitialized) :Promise<void> {

        console.log("OnBlazorControllerInitialized", obj);

    }

}
