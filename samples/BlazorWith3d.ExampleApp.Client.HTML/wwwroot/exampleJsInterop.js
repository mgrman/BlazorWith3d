export function InitializeGlobalMouseEvents(dotnetObject, onMouseMoveMethodName, onMouseUpMethodName) {
    var onMoveCallback= function (o) {

        dotnetObject.invokeMethodAsync(onMouseMoveMethodName,o.pageX, o.pageY);
    } 
    var onUpCallback= function (o) {
        dotnetObject.invokeMethodAsync(onMouseUpMethodName)
    }

    window.addEventListener("mousemove", onMoveCallback);
    window.addEventListener("mouseup", onUpCallback);

    let disposeTracker = {}
    disposeTracker.Dispose=function (){

        window.removeEventListener("mousemove", onMoveCallback);
        window.removeEventListener("mouseup", onUpCallback);
    }
    
    return disposeTracker;
}
