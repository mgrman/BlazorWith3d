import * as THREE from 'three';
import {GLTF, GLTFLoader} from 'three/examples/jsm/loaders/GLTFLoader.js';
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
    BlocksOnGridUnityApi, BlocksOnGridUnityApi_MethodInvoker_DirectInterop,
    IBlocksOnGridUnityApi, IBlocksOnGridUnityApi_EventHandler,
    IBlocksOnGridUnityApi_MethodInvoker
} from "com.blazorwith3d.exampleapp.client.shared/memorypack/BlocksOnGridUnityApi";
import {BlazorBinaryApi} from "com.blazorwith3d.exampleapp.client.shared/BlazorBinaryApi";
import { RequestRaycast } from "com.blazorwith3d.exampleapp.client.shared/memorypack/RequestRaycast";
import { RequestScreenToWorldRay } from "com.blazorwith3d.exampleapp.client.shared/memorypack/RequestScreenToWorldRay";
import { ScreenToWorldRayResponse } from "com.blazorwith3d.exampleapp.client.shared/memorypack/ScreenToWorldRayResponse";
import { RaycastResponse } from "com.blazorwith3d.exampleapp.client.shared/memorypack/RaycastResponse";


export function InitializeApp_BinaryApi(canvas: HTMLCanvasElement, dotnetObject: any, onMessageReceivedMethodName: string) {
    var sendMessageCallback: (msgBytes: Uint8Array) => Promise<any> = msgBytes => dotnetObject.invokeMethodAsync(onMessageReceivedMethodName, msgBytes);


    var binaryApi= new BlazorBinaryApi(sendMessageCallback);
    var blazorApp=new BlocksOnGridUnityApi(binaryApi);
    
    var app= new DebugApp(canvas, blazorApp);

    blazorApp.SetUpUsingEventHandler(app);
    
    return app;
}

export function InitializeApp_DirectInterop(canvas: HTMLCanvasElement, dotnetObject: any) {

    var sendMessageCallback: (methodName:string, msg: any) => Promise<any> = (methodName, msg)  => dotnetObject.invokeMethodAsync(methodName, msg);

    return new DebugApp(canvas, new BlocksOnGridUnityApi_MethodInvoker_DirectInterop(dotnetObject));
}

export class DebugApp implements IBlocksOnGridUnityApi_EventHandler{
    private templates:  { [id: number] : {template: AddBlockTemplate, visuals: GLTF} } = {};
    private instances:  { [id: number] : {instance: AddBlockInstance, mesh: THREE.Mesh, visuals: THREE.Group}} = {};

    private camera: THREE.PerspectiveCamera;
    scene: THREE.Scene;
    renderer: THREE.WebGLRenderer;
    private raycaster: THREE.Raycaster;
    private canvas: HTMLCanvasElement;
    private _methodInvoker: IBlocksOnGridUnityApi_MethodInvoker;

    constructor(canvas: HTMLCanvasElement, methodInvoker: IBlocksOnGridUnityApi_MethodInvoker) {

        this.canvas=canvas;
        this._methodInvoker = methodInvoker;
        canvas.style.width = "100%";
        canvas.style.height = "100%";
        canvas.width = canvas.offsetWidth;
        canvas.height = canvas.offsetHeight;

        this.camera = new THREE.PerspectiveCamera( 60, canvas.width / canvas.height, 0.1, 100 );
        this.camera.position.z = 10;
        // the camera in ThreeJS is looking down negativeZ direciton, so no need to rotate
        this.camera.setRotationFromEuler( new THREE.Euler(THREE.MathUtils.degToRad(0),0,0));

        this.scene = new THREE.Scene();

        const directionalLight = new THREE.DirectionalLight( 0xffffff, 1 );
        directionalLight.position.set(0,0,0);

        directionalLight.target.position.copy(new THREE.Vector3(0, 0, -1).applyEuler(new THREE.Euler(THREE.MathUtils.degToRad(-30), THREE.MathUtils.degToRad(-50), THREE.MathUtils.degToRad(0))));

        this.scene.add( directionalLight );
        this.scene.add( directionalLight.target );

        this.renderer = new THREE.WebGLRenderer( { antialias: true, canvas:canvas } );
        this.renderer.setPixelRatio( window.devicePixelRatio );
        this.renderer.setSize( canvas.width, canvas.height );

        this.renderer.setAnimationLoop( ()=>{

            this.renderer.render( this.scene, this.camera );

        } );


        this.raycaster = new THREE.Raycaster();

        this._methodInvoker.InvokeUnityAppInitialized(new UnityAppInitialized()).then(_ => console.log("UnityAppInitialized invoked"));
    }

    public Quit(): void {
        console.log("Quit called");
    }

    public OnUpdateBlockInstance(obj: UpdateBlockInstance) {
        console.log("BlockPoseChangeValidated", obj);


        const {instance, mesh} = this.instances[obj.blockId];

        instance.position = obj.position;
        instance.rotationZ = obj.rotationZ;
        this.UpdateMeshPosition( obj.blockId);
    }


    public OnRemoveBlockTemplate(obj: RemoveBlockTemplate) {
        console.log("RemoveBlockTemplate", obj);

        delete this.templates[obj.templateId];
    }

    public OnRemoveBlockInstance(obj: RemoveBlockInstance) {
        console.log("RemoveBlockInstance", obj);


        const {instance, mesh} = this.instances[obj.blockId];
        this.scene.remove(mesh);
        delete this.instances[obj.blockId];
    }

    public OnAddBlockInstance(obj: AddBlockInstance) {
        console.log("AddBlockInstance", obj);

        var {template, visuals } = this.templates[obj.templateId];


        const geometry = new THREE.BoxGeometry(template.size.x,template.size.y,template.size.z);

        const material = new THREE.MeshPhongMaterial( { color: 0xaaaaaa   } );

        var mesh = new THREE.Mesh( geometry, material );
        mesh.position.set(0, 0, template.size.z / 2);
        this.scene.add( mesh );



        this.instances[obj.blockId]={instance:obj, mesh, visuals:null};

        this.UpdateMeshPosition(obj.blockId);


        if(visuals!=null) {
            this.InstantiateGlft(visuals, obj, mesh);
        }
    }

    private UpdateMeshPosition( blockId: number)  {

        var {instance, mesh, visuals} = this.instances[blockId];

        mesh.position.set(instance.position.x, instance.position.y, mesh.position.z);
        mesh.rotation.set(0, 0, THREE.MathUtils.degToRad(instance.rotationZ));

        if(visuals!=null) {

            visuals.position.set(instance.position.x, instance.position.y,0);
            visuals.rotation.set(0, 0, THREE.MathUtils.degToRad(instance.rotationZ));
        }
    }

    public OnRequestScreenToWorldRay(msg: RequestScreenToWorldRay): void {

        var a=msg;


        const pointer = new THREE.Vector2();

        // convert to ThreeJS screen coordinates
        pointer.x = ( msg.screen.x / this.canvas.width ) * 2 - 1;
        pointer.y = - ( msg.screen.y / this.canvas.height ) * 2 + 1;



        this.raycaster.setFromCamera( pointer, this.camera );
        const ray=this.raycaster.ray;

        this._methodInvoker.InvokeScreenToWorldRayResponse({
            requestId:msg.requestId,
            ray: {
                origin: { x: ray.origin.x, y: ray.origin.y, z: ray.origin.z },
                direction: { x: ray.direction.x, y: ray.direction.y, z: ray.direction.z }
            }
        })
    }

    public OnRequestRaycast(msg: RequestRaycast): void {

        var origin=msg.ray.origin;
        var direction=msg.ray.direction;

        this.raycaster.set(new THREE.Vector3(origin.x,origin.y,origin.z),new THREE.Vector3(direction.x,direction.y,direction.z));


        const intersects = this.raycaster.intersectObjects( this.scene.children, false );
        var response = new RaycastResponse();
        response.requestId = msg.requestId;
        response.hitWorld = origin;

        if ( intersects.length > 0 ) {

            let instance:AddBlockInstance=null;
            for(let t in this.instances) {
                if (this.instances[t].mesh === intersects[0].object) {
                    instance = this.instances[t].instance;
                }
            }
            response.hitBlockId = instance.blockId;
            response.hitWorld = intersects[0].point;
        }
        this._methodInvoker.InvokeRaycastResponse(response);
    }
    public OnAddBlockTemplate(template: AddBlockTemplate) {
        console.log("AddBlockTemplate", template);

        this.templates[template.templateId]={template, visuals:null};
        if(template.visualsUri!=null) {
            const loader = new GLTFLoader();

            loader.load(
                template.visualsUri,
                (gltf) => {
                    // called when the resource is loaded

                    if(Object.hasOwn(this.templates, template.templateId) ) {
                        this.templates[template.templateId].visuals=gltf;
                    }

                    for (let i in this.instances) {

                        let instance=this.instances[i];
                        if(instance.instance.templateId == template.templateId) {

                            this.InstantiateGlft(gltf,instance.instance, instance.mesh);
                        }

                    }

                },
                (xhr) => {
                    // called while loading is progressing
                    console.log(`${(xhr.loaded / xhr.total * 100)}% loaded`);
                },
                (error) => {
                    // called when loading has errors
                    console.error('An error happened', error);
                },
            );
        }


    }

    private InstantiateGlft(gltf: GLTF, instance: AddBlockInstance, mesh: THREE.Mesh) {

        mesh.visible=false;

        const model = gltf.scene.clone();

        model.position.set(mesh.position.x,mesh.position.y,0);
        model.rotation.copy(mesh.rotation);
        this.scene.add(model);

        this.instances[instance.blockId].visuals=model;
    }

    public OnPerfCheck(obj: PerfCheck)
    {
        this._methodInvoker.InvokePerfCheck(
            {
                aaa: obj.aaa,
                bbb: obj.bbb,
                ccc: obj.ccc,
                ddd: obj.ddd,
                id: obj.id
            }).then();
    }

    public OnBlazorControllerInitialized(obj: BlazorControllerInitialized) {

        console.log("OnBlazorControllerInitialized", obj);

    }

}
