import * as THREE from 'three';
import {GLTF, GLTFLoader} from 'three/examples/jsm/loaders/GLTFLoader.js';
import {AddBlockInstance} from "com.blazorwith3d.exampleapp.client.shared/memorypack/AddBlockInstance";
import {PerfCheck} from "com.blazorwith3d.exampleapp.client.shared/memorypack/PerfCheck";
import {AddBlockTemplate} from "com.blazorwith3d.exampleapp.client.shared/memorypack/AddBlockTemplate";
import {RemoveBlockInstance} from "com.blazorwith3d.exampleapp.client.shared/memorypack/RemoveBlockInstance";
import {RemoveBlockTemplate} from "com.blazorwith3d.exampleapp.client.shared/memorypack/RemoveBlockTemplate";
import { RendererInitialized } from "com.blazorwith3d.exampleapp.client.shared/memorypack/RendererInitialized";
import {
    BlocksOnGrid3DControllerOverDirectInterop,
    BlocksOnGrid3DControllerOverBinaryApi, IBlocksOnGrid3DController, IBlocksOnGrid3DRenderer
} from "com.blazorwith3d.exampleapp.client.shared/memorypack/IBlocksOnGrid3DController";
import {BlazorBinaryApiWithResponse} from "com.blazorwith3d.exampleapp.client.shared/BlazorBinaryApiWithResponse";
import { RequestRaycast } from "com.blazorwith3d.exampleapp.client.shared/memorypack/RequestRaycast";
import { RequestScreenToWorldRay } from "com.blazorwith3d.exampleapp.client.shared/memorypack/RequestScreenToWorldRay";
import { ScreenToWorldRayResponse } from "com.blazorwith3d.exampleapp.client.shared/memorypack/ScreenToWorldRayResponse";
import { RaycastResponse } from "com.blazorwith3d.exampleapp.client.shared/memorypack/RaycastResponse";
import { TriggerTestToBlazor } from 'com.blazorwith3d.exampleapp.client.shared/memorypack/TriggerTestToBlazor';
import { PackableVector2 } from 'com.blazorwith3d.exampleapp.client.shared/memorypack/PackableVector2';
import { RendererInitializationInfo } from 'com.blazorwith3d.exampleapp.client.shared/memorypack/RendererInitializationInfo';
import {DirectionalLight, SRGBColorSpace} from "three";


export function InitializeApp(canvas: HTMLCanvasElement, _: any, dotnetObject: any, onMessageReceivedMethodName: string, onMessageReceivedWithResponseMethodName: string) {
    let sendMessageCallback: (msgBytes: Uint8Array) => Promise<any> = msgBytes => dotnetObject.invokeMethodAsync(onMessageReceivedMethodName, msgBytes);
    let sendMessageWithResponseCallback: (msgBytes: Uint8Array) => Promise<Uint8Array> = msgBytes => dotnetObject.invokeMethodAsync(onMessageReceivedWithResponseMethodName, msgBytes);

    let renderer = new DebugApp(canvas);

    let binaryApi = new BlazorBinaryApiWithResponse(sendMessageCallback, sendMessageWithResponseCallback);
    let controller = new BlocksOnGrid3DControllerOverBinaryApi(binaryApi,renderer);

    renderer.Initialize(controller);

    let appAsAny: any = renderer;
    appAsAny.ProcessMessage = (msg: Uint8Array, offset: number, count: number) => {

        msg = msg.subarray(offset, count);
        return binaryApi.mainMessageHandler(msg);
    }
    appAsAny.ProcessMessageWithResponse = (msg: Uint8Array, offset: number, count: number) => {
        msg = msg.subarray(offset, count);
        return binaryApi.mainMessageWithResponseHandler(msg);
    }
    return appAsAny;
}

export class DebugApp implements IBlocksOnGrid3DRenderer {
    private templates: { [id: number]: any } = {};
    private instances: { [id: number]: { instance: AddBlockInstance, mesh: THREE.Mesh, visuals: THREE.Group } } = {};

    private camera: THREE.PerspectiveCamera;
    private scene: THREE.Scene;
    private renderer: THREE.WebGLRenderer;
    private raycaster: THREE.Raycaster;
    private canvas: HTMLCanvasElement;
    private _methodInvoker: IBlocksOnGrid3DController;
    resizeEvent: (_: any) => void;
    private directionalLight: DirectionalLight;

    constructor(canvas: HTMLCanvasElement) {

        this.canvas = canvas;
        this.canvas.style.width = "100%";
        this.canvas.style.height = "100%";
        this.canvas.width = this.canvas.offsetWidth;
        this.canvas.height = this.canvas.offsetHeight;

        this.camera = new THREE.PerspectiveCamera(60, this.canvas.width / this.canvas.height, 0.1, 100);
        this.camera.position.z = 10;
        // the camera in ThreeJS is looking down negativeZ direciton, so no need to rotate
        this.camera.setRotationFromEuler(new THREE.Euler(THREE.MathUtils.degToRad(0), 0, 0));

        this.scene = new THREE.Scene();

        this.directionalLight = new THREE.DirectionalLight(0xffffff, 3);
        this.directionalLight.position.set(0, 0, 0);
        this.directionalLight.target.position.copy(new THREE.Vector3(0, 0, -1));

        this.scene.add(this.directionalLight);
        this.scene.add(this.directionalLight.target);

        this.renderer = new THREE.WebGLRenderer({antialias: true, canvas: this.canvas});
        this.renderer.setSize(this.canvas.width, this.canvas.height);
        this.renderer.setPixelRatio(window.devicePixelRatio);
        
        this.renderer.setAnimationLoop(() => {

            this.renderer.render(this.scene, this.camera);

        });
        
        this.resizeEvent = (_) => this.HandleResize();
        window.addEventListener('resize',this.resizeEvent );

        this.raycaster = new THREE.Raycaster();
    }

    public Initialize(controller:IBlocksOnGrid3DController) {
        this._methodInvoker = controller;
    }
    
    private HandleResize():void {

        this.canvas.style.width = "100%";
        this.canvas.style.height = "100%";
        this.canvas.width = this.canvas.offsetWidth;
        this.canvas.height = this.canvas.offsetHeight;
        
        // Update camera
        this.camera.aspect = this.canvas.width / this.canvas.height
        this.camera.updateProjectionMatrix()

        // Update renderer
        this.renderer.setSize(this.canvas.width, this.canvas.height);
        this.renderer.setPixelRatio(window.devicePixelRatio);

    }
    
    public async InitializeRenderer(msg: RendererInitializationInfo): Promise<void> {

        this.scene.background= new THREE.Color().setRGB( msg.backgroundColor.r, msg.backgroundColor.g, msg.backgroundColor.b, SRGBColorSpace );



        this.camera.fov = msg.requestedCameraFoV;
        this.camera.position.x = msg.requestedCameraPosition.x;
        this.camera.position.y = msg.requestedCameraPosition.y;
        this.camera.position.z = msg.requestedCameraPosition.z;
        // the camera in ThreeJS is looking down negativeZ direciton, so no need to rotate
        this.camera.setRotationFromEuler(new THREE.Euler(THREE.MathUtils.degToRad(msg.requestedCameraRotation.x), THREE.MathUtils.degToRad(msg.requestedCameraRotation.y), THREE.MathUtils.degToRad(-msg.requestedCameraRotation.z),"ZXY") );


        this.directionalLight.target.position.copy(new THREE.Vector3(0, 0, -1).applyEuler(new THREE.Euler(THREE.MathUtils.degToRad(msg.requestedDirectionalLightRotation.x), THREE.MathUtils.degToRad(msg.requestedDirectionalLightRotation.y), THREE.MathUtils.degToRad(-msg.requestedDirectionalLightRotation.z),"ZXY")));


        this._methodInvoker.OnRendererInitialized(new RendererInitialized(), this).then(_ => console.log("UnityAppInitialized invoked"));
    }

    public async InvokeTriggerTestToBlazor(_: TriggerTestToBlazor): Promise<void> {
        
        setTimeout(async ()=> {
            var response=await this._methodInvoker.OnTestToBlazor({ id : 13  })


            
            
            if (response.id != 13)
            {
                console.log("TriggerTestToBlazor is failure");
            }
            console.log("TriggerTestToBlazor is done");
        } ,1000)
    }

    public Quit(): void {
        console.log("Quit called");

        window.removeEventListener('resize',this.resizeEvent );
    }

    public async InvokeUpdateBlockInstance(blockId: number| null, position: PackableVector2, rotationZ: number) : Promise<any> {
        console.log("OnUpdateBlockInstance", blockId, position, rotationZ);


        const {instance, mesh} = this.instances[blockId];

        instance.position = position;
        instance.rotationZ = rotationZ;
        this.UpdateMeshPosition(blockId);
    }


    public async InvokeRemoveBlockTemplate(obj: RemoveBlockTemplate): Promise<any>  {
        console.log("RemoveBlockTemplate", obj);

        delete this.templates[obj.templateId];
    }

    public async InvokeRemoveBlockInstance(obj: RemoveBlockInstance): Promise<any>  {
        console.log("RemoveBlockInstance", obj);


        const {instance, mesh, visuals} = this.instances[obj.blockId];
        this.scene.remove(mesh);
        if(visuals)
        {
            this.scene.remove(visuals);
        }
        delete this.instances[obj.blockId];
    }

    public async InvokeAddBlockInstance(obj: AddBlockInstance): Promise<any>  {
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
        // rotation is negative as ThreeJS rotates CCW and we want CW direction
        mesh.rotation.set(0, 0, -THREE.MathUtils.degToRad(instance.rotationZ),"ZXY");

        if(visuals!=null) {

            visuals.position.set(instance.position.x, instance.position.y,0);
            visuals.rotation.set(0, 0, -THREE.MathUtils.degToRad(instance.rotationZ),"ZXY");
        }
    }

    public async InvokeRequestScreenToWorldRay(msg: RequestScreenToWorldRay): Promise<ScreenToWorldRayResponse> {
        const pointer = new THREE.Vector2();

        // convert to ThreeJS screen coordinates
        const virtualPixelWidth=this.canvas.width/this.renderer.getPixelRatio();
        const virtualPixelHeight=this.canvas.height/this.renderer.getPixelRatio();
        pointer.x = ( msg.screen.x / virtualPixelWidth ) * 2 - 1;
        pointer.y = - ( msg.screen.y /virtualPixelHeight) * 2 + 1;
        
        this.raycaster.setFromCamera( pointer, this.camera );
        const ray=this.raycaster.ray;

        return {
            ray: {
                origin: { x: ray.origin.x, y: ray.origin.y, z: ray.origin.z },
                direction: { x: ray.direction.x, y: ray.direction.y, z: ray.direction.z }
            }
        }
    }

    public async InvokeRequestRaycast(msg: RequestRaycast): Promise<RaycastResponse> {

        var origin=msg.ray.origin;
        var direction=msg.ray.direction;

        this.raycaster.set(new THREE.Vector3(origin.x,origin.y,origin.z),new THREE.Vector3(direction.x,direction.y,direction.z));


        const intersects = this.raycaster.intersectObjects( this.scene.children, false );
        var response = new RaycastResponse();
        response.hitWorld = origin;

        if ( intersects.length > 0 ) {

            let instance:AddBlockInstance=null;
            for(let t in this.instances) {
                if (this.instances[t].mesh === intersects[0].object) {
                    instance = this.instances[t].instance;
                }
            }
            response.isBlockHit = true;
            response.hitBlockId = instance.blockId;
            response.hitWorld = intersects[0].point;
        }
        return response;
    }
    public async InvokeAddBlockTemplate(template: AddBlockTemplate):Promise<any>  {
        console.log("AddBlockTemplate", template);

        this.templates[template.templateId]={template, visuals:null};
        if(template.visuals3dUri!=null) {
            const loader = new GLTFLoader();

            loader.load(
                template.visuals3dUri,
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

    public async InvokePerfCheck(obj: PerfCheck) :Promise<PerfCheck>
    {
       return  obj;
    }
}
