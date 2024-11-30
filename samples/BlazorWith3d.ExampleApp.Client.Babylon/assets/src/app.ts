import "@babylonjs/core/Debug/debugLayer";
import "@babylonjs/inspector";
import {
    Engine,
    Scene,
    ArcRotateCamera,
    Vector3,
    HemisphericLight,
    Mesh,
    MeshBuilder,
    PointerDragBehavior, Camera, FreeCamera, Vector2, Tools, Matrix, PointLight, DirectionalLight
} from "@babylonjs/core";
import {AddBlockInstance} from "./memorypack/AddBlockInstance";
import {BlazorControllerInitialized} from "./memorypack/BlazorControllerInitialized";
import {PerfCheck} from "./memorypack/PerfCheck";
import {AddBlockTemplate} from "./memorypack/AddBlockTemplate";
import {RemoveBlockInstance} from "./memorypack/RemoveBlockInstance";
import {RemoveBlockTemplate} from "./memorypack/RemoveBlockTemplate";
import {StartDraggingBlock} from "./memorypack/StartDraggingBlock";
import {BlockPoseChangeValidated} from "./memorypack/BlockPoseChangeValidated";
import {MemoryPackWriter} from "./memorypack/MemoryPackWriter";
import {UnityAppInitialized} from "./memorypack/UnityAppInitialized";
import {BlockPoseChanging} from "./memorypack/BlockPoseChanging";
import {BlockPoseChanged} from "./memorypack/BlockPoseChanged";

export function InitializeBabylonApp(canvas: HTMLCanvasElement, dotnetObject: any, onMessageReceivedMethodName:string) {

    var sendMessageCallback:  (msgBytes: Uint8Array) => Promise<any>  =msgBytes=>dotnetObject.invokeMethodAsync(onMessageReceivedMethodName, msgBytes);
    
    return new DebugApp(canvas,sendMessageCallback);
}


export class DebugApp {
    private _sendMessage: (msgBytes: Uint8Array) => Promise<any>;
    private scene: Scene;
    private templates: Array<AddBlockTemplate>= new Array<AddBlockTemplate>();
    private instances: Array<[instance:AddBlockInstance, mesh: Mesh, drag: PointerDragBehavior]> = new Array<[instance: AddBlockInstance, mesh: Mesh, drag: PointerDragBehavior]>();

    private changingRequestId: number=0;
    private plane: Mesh; 
    
    constructor(canvas: HTMLCanvasElement, sendMessage: (msgBytes: Uint8Array) => Promise<any>) {
        this._sendMessage = sendMessage;

        // initialize babylon scene and engine
        var engine = new Engine(canvas, true);
        this.scene = new Scene(engine);

        var camera = new FreeCamera("Camera", new Vector3(0,0,10), this.scene);
        camera.setTarget(Vector3.Zero());
        var light1 = new DirectionalLight("light1", new Vector3(0, 3, 0), this.scene);
        light1.direction= new Vector3(-1.144162,-2.727118,-1.981747)

        this.plane = MeshBuilder.CreatePlane("plane", {
            width: 10,
            height: 10,
            sideOrientation: Mesh.DOUBLESIDE

        }, this.scene);
        this.plane.rotation=new Vector3(Tools.ToRadians(4.92),Tools.ToRadians(159.77-180),Tools.ToRadians(180));
        

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

        
        this.InvokeUnityAppInitialized(new UnityAppInitialized()).then(_ => console.log("UnityAppInitialized invoked"));
    }

    public ProcessMessage(msg: Uint8Array): void {
        let msgId = msg[0];
        let buffer = msg.slice(1);

        var dst = new ArrayBuffer(buffer.byteLength);
        new Uint8Array(dst).set(buffer);

        try {
        switch (msgId) {
            case 0: {
                const obj: BlazorControllerInitialized = BlazorControllerInitialized.deserialize(dst);
                this.OnBlazorControllerInitialized?.(obj);
                break;
            }
            case 1: {
                const obj = PerfCheck.deserialize(dst);
                this.OnPerfCheck?.(obj);
                break;
            }
            case 2: {
                const obj: AddBlockTemplate = AddBlockTemplate.deserialize(dst);
                this.OnAddBlockTemplate?.(obj);
                break;
            }
            case 3: {
                const obj: AddBlockInstance = AddBlockInstance.deserialize(dst);
                this.OnAddBlockInstance?.(obj);
                break;
            }
            case 4: {
                const obj: RemoveBlockInstance = RemoveBlockInstance.deserialize(dst);
                this.OnRemoveBlockInstance?.(obj);
                break;
            }
            case 5: {
                const obj: RemoveBlockTemplate = RemoveBlockTemplate.deserialize(dst);
                this.OnRemoveBlockTemplate?.(obj);
                break;
            }
            case 6: {
                const obj: StartDraggingBlock = StartDraggingBlock.deserialize(dst);
                this.OnStartDraggingBlock?.(obj);
                break;
            }
            case 7: {
                const obj: BlockPoseChangeValidated = BlockPoseChangeValidated.deserialize(dst);
                this.OnBlockPoseChangeValidated?.(obj);
                break;
            }
        }
        } catch (e) {
            console.log(e)
        }
    }
    
    public Quit(): void {
        console.log("Quit called");
    }

    protected OnBlockPoseChangeValidated(obj: BlockPoseChangeValidated) {
        console.log("BlockPoseChangeValidated", obj);


        const [instance, mesh, drag]  = this.instances.find(o => o[0].blockId === obj.blockId);

        instance.positionX=obj.newPositionX;
        instance.positionY=obj.newPositionY;
        instance.rotationZ=obj.newRotationZ;
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


        const [instance, mesh, drag]  = this.instances.find(o => o[0].blockId === obj.blockId);
        this.instances = this.instances.filter(o => o[0].blockId !== obj.blockId);
        
        this.scene.removeMesh(mesh);
    }

    protected OnAddBlockInstance(obj: AddBlockInstance) {
        console.log("AddBlockInstance", obj);
        
       var template= this.templates.find(o => o.templateId === obj.templateId); 
        
        var mesh: Mesh = MeshBuilder.CreateBox("box"+obj.blockId, {
            width: template.sizeX,
            height: template.sizeY,
            depth: template.sizeZ
        }, this.scene);
        mesh.parent=this.plane;
        mesh.position = new Vector3(0, 0, template.sizeZ / 2);
        this.UpdateMeshPosition(mesh, obj);

        var pointerDragBehavior = new PointerDragBehavior({ dragPlaneNormal: new Vector3(0, 0, 1) });
        pointerDragBehavior.useObjectOrientationForDragging = false;
        pointerDragBehavior.updateDragPlane = false;
        
        var initialPoint: Vector2;


        const matrix = Matrix.Invert(this.plane.computeWorldMatrix(true));
        
        pointerDragBehavior.onDragStartObservable.add((event) => {
            console.log("dragStart");
            console.log(event);
            
            var localPoint= Vector3.TransformCoordinates(event.dragPlanePoint, matrix);

            initialPoint= new Vector2(localPoint.x- obj.positionX,localPoint.y-obj.positionY);
        });
        pointerDragBehavior.onDragObservable.add((event) => {
            console.log("drag");
            console.log(event);
            
            var localPoint= Vector3.TransformCoordinates(event.dragPlanePoint, matrix);

            this.InvokeBlockPoseChanging({

                blockId: obj.blockId,
                changingRequestId: this.changingRequestId++,
                positionX: localPoint.x-initialPoint.x,
                positionY: localPoint.y-initialPoint.y,
                rotationZ: obj.rotationZ
            }).then();
        });
        pointerDragBehavior.onDragEndObservable.add((event) => {
            console.log("dragEnd");
            console.log(event);
            this.InvokeBlockPoseChanged( {
                blockId : obj.blockId,
                positionX : obj.positionX,
                positionY : obj.positionY,
                rotationZ : obj.rotationZ
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
        this.InvokePerfCheck(
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

        this.templates= new Array<AddBlockTemplate>();
        
        
        for(const i of this.instances) {
            const [instance, mesh, drag] = i;

            this.scene.removeMesh(mesh);
        }
        
        this.instances= new Array<[instance: AddBlockInstance, mesh: Mesh, drag: PointerDragBehavior]>();

    }

    protected async InvokePerfCheck(msg: PerfCheck): Promise<void> {
        await this.sendMessage(0, w => PerfCheck.serializeCore(w, msg));
    }

    protected async InvokeUnityAppInitialized(msg: UnityAppInitialized): Promise<void> {
        await this.sendMessage(1, w => UnityAppInitialized.serializeCore(w, msg));
    }

    protected async InvokeBlockPoseChanging(msg: BlockPoseChanging): Promise<void> {
        await this.sendMessage(2, w => BlockPoseChanging.serializeCore(w, msg));
    }

    protected async InvokeBlockPoseChanged(msg: BlockPoseChanged): Promise<void> {
        await this.sendMessage(3, w => BlockPoseChanged.serializeCore(w, msg));
    }

    private previousMessage: Promise<void> = Promise.resolve();

    private async sendMessage(messageId: number, messageSerializeCore: (writer: MemoryPackWriter) => any): Promise<void> {
        try {
            const writer = MemoryPackWriter.getSharedInstance();
            writer.writeInt8(messageId);
            messageSerializeCore(writer);
            const encodedMessage = writer.toArray();

            await this.previousMessage;
            this.previousMessage = this._sendMessage(encodedMessage);
        } catch (ex) {
            throw ex;
        }
    }
}
