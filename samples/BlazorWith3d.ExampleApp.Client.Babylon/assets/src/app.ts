import "@babylonjs/core/Debug/debugLayer";
import "@babylonjs/inspector";
import { Engine, Scene, ArcRotateCamera, Vector3, HemisphericLight, Mesh, MeshBuilder } from "@babylonjs/core";
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
    private templates: Array<AddBlockTemplate>;
    private instances: Array<[instance:AddBlockInstance, mesh: Mesh]>;

    constructor(canvas: HTMLCanvasElement, sendMessage: (msgBytes: Uint8Array) => Promise<any>) {
        this._sendMessage = sendMessage;

        // initialize babylon scene and engine
        var engine = new Engine(canvas, true);
        this.scene = new Scene(engine);

        var camera: ArcRotateCamera = new ArcRotateCamera("Camera", Math.PI / 2, Math.PI / 2, 2, Vector3.Zero(), this.scene);
        camera.attachControl(canvas, true);
        var light1: HemisphericLight = new HemisphericLight("light1", new Vector3(1, 1, 0), this.scene);

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

        switch (msgId) {
            case 0: {
                const obj: BlazorControllerInitialized = BlazorControllerInitialized.deserialize(dst);
                this.OnBlazorControllerInitialized?.(obj);
                break;
            }
            case 1: {
                let obj: PerfCheck;
                try {
                    obj = PerfCheck.deserialize(dst);
                } catch (e) {
                    console.log(e)
                }
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
    }
    
    public Quit(): void {
        console.log("Quit called");
    }

    protected OnBlockPoseChangeValidated(obj: BlockPoseChangeValidated) {
        console.log("BlockPoseChangeValidated", obj);
    }

    protected OnStartDraggingBlock(obj: StartDraggingBlock) {
        console.log("StartDraggingBlock", obj);
    }

    protected OnRemoveBlockTemplate(obj: RemoveBlockTemplate) {
        console.log("RemoveBlockTemplate", obj);
        this.templates = this.templates.filter(o => o.templateId !== obj.templateId);
    }

    protected OnRemoveBlockInstance(obj: RemoveBlockInstance) {
        console.log("RemoveBlockInstance", obj);
        this.instances = this.instances.filter(o => o.blockId !== obj.blockId);
    }

    protected OnAddBlockInstance(obj: AddBlockInstance) {
        console.log("AddBlockInstance", obj);
        this.instances.push(obj);
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
