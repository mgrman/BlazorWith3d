// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.
export function InitializeUnityApi (unityInstance, onMessageReceivedCallback ) {

  let unityApi = {}
  
  // void BlazorApi_OnMessageFromUnityHandler(byte[] message)
  unityInstance.Module["BlazorApi_OnMessageFromUnityHandler"] = function (msgBytes) {
    try {
      onMessageReceivedCallback(msgBytes);
    } catch (err) {
      console.error(err);
    }
  }

  unityApi.SendMessage = function (msgBytes) {
    // void BlazorApi_SendMessageToUnity(byte[] message)
    try {
      unityInstance.Module["BlazorApi_SendMessageToUnity"](msgBytes);
    } catch (err) {
      console.error(err);
    }
  };
  
  unityApi.Quit=async function (){
   await unityInstance.Quit()
   
  }

  return unityApi;
}


export function showUnity(buildUrl,container, dotnetObject, onMessageReceivedMethodName ) {

  var canvas = container.querySelector("#unity-canvas");

  // Shows a temporary message banner/ribbon for a few seconds, or
  // a permanent error message on top of the canvas if type=='error'.
  // If type=='warning', a yellow highlight color is used.
  // Modify or remove this function to customize the visually presented
  // way that non-critical warnings and error messages are presented to the
  // user.
  function unityShowBanner(msg, type) {
    var warningBanner = container.querySelector("#unity-warning");

    function updateBannerVisibility() {
      warningBanner.style.display = warningBanner.children.length ? 'block' : 'none';
    }

    var div = document.createElement('div');
    div.innerHTML = msg;
    warningBanner.appendChild(div);
    if (type == 'error') div.style = 'background: red; padding: 10px;';
    else {
      if (type == 'warning') div.style = 'background: yellow; padding: 10px;';
      setTimeout(function () {
        warningBanner.removeChild(div);
        updateBannerVisibility();
      }, 5000);
    }
    updateBannerVisibility();
  }

  //var buildUrl = "./_content/BlazorWith3d.Unity";
  var loaderUrl = buildUrl + "/Build.loader.js";
  var config = {
    arguments: [],
    dataUrl: buildUrl + "/Build.data",
    frameworkUrl: buildUrl + "/Build.framework.js",
    workerUrl: buildUrl + "/Build.worker.js",
    codeUrl: buildUrl + "/Build.wasm",
    streamingAssetsUrl: "StreamingAssets",
    companyName: "DefaultCompany",
    productName: "UnityForBlazor",
    productVersion: "0.1.0",
    showBanner: unityShowBanner,
    matchWebGLToCanvasSize: true,
    devicePixelRatio: 1
  };

  // If you would like all file writes inside Unity Application.persistentDataPath
  // directory to automatically persist so that the contents are remembered when
  // the user revisits the site the next time, uncomment the following line:
  // config.autoSyncPersistentDataPath = true;
  // This autosyncing is currently not the default behavior to avoid regressing
  // existing user projects that might rely on the earlier manual
  // JS_FileSystem_Sync() behavior, but in future Unity version, this will be
  // expected to change.

  container.querySelector("#unity-loading-bar").style.display = "block";

  return new Promise((resolve, reject) => {
    var script = document.createElement("script");
    script.src = loaderUrl;
    script.onload = () => {
      script.onload=null;
      createUnityInstance(canvas, config, (progress) => {
        container.querySelector("#unity-progress-bar-full").style.width = 100 * progress + "%";
      }).then((unityInstance) => {
        container.querySelector("#unity-loading-bar").style.display = "none";


        var previousMessage=Promise.resolve();
        var unityApi = InitializeUnityApi(unityInstance, async function(msgBytes){
          await previousMessage;
          previousMessage=dotnetObject.invokeMethodAsync(onMessageReceivedMethodName, msgBytes)
        });
        resolve(unityApi);

        // container.querySelector("#unity-fullscreen-button").onclick = () => {
        //   unityInstance.SetFullscreen(1);
        // };

        // Unloading web content from DOM so that browser GC can run can be tricky to get right.
        // This code snippet shows how to correctly implement a Unity content Unload mechanism to a web page.

        // Unloading Unity content enables a web page to reclaim the memory used by Unity, e.g. for
        // the purpose of later loading another Unity content instance on the _same_ web page.

        // When using this functionality, take caution to carefully make sure to clear all JavaScript code,
        // DOM element and event handler references to the old content you may have retained, or
        // otherwise the browser's garbage collector will be unable to reclaim the old page.

        // N.b. Unity content does _not_ need to be manually unloaded when the user is navigating away from
        // the current page to another web page. The browser will take care to clear memory of old visited
        // pages automatically. This functionality is only needed if you want to switch between loading
        // multiple Unity builds on a single web page.
        // var quit = document.createElement("button");
        // quit.style = "margin-left: 5px; background-color: lightgray; border: none; padding: 5px; cursor: pointer";
        // quit.innerHTML = "Unload";
        // container.querySelector("#unity-build-title").appendChild(quit);
        // quit.onclick = () => {
        //   // Quit Unity application execution
        //   unityInstance.Quit().then(() => {
        //     // Remove DOM elements from the page so GC can run
        //     container.remove();
        //     canvas = null;
        //   });
        // };
      }).catch((message) => {
        alert(message);
        reject(message);
      });
    };

    container.appendChild(script);
  });
}