export function InitializeGlobalMouseEvents(elementTarget, dotnetObject, onMouseMoveMethodName, onMouseUpMethodName) {
    var onMoveCallback= function (o) {

        var rect = elementTarget.getBoundingClientRect();
        var x = o.pageX - rect.left;
        var y = o.pageY - rect.top;
        
        dotnetObject.invokeMethodAsync(onMouseMoveMethodName,x, y);
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

export function ConvertPageToOffset(elementTarget, pageX, pageY) {

    var rect = elementTarget.getBoundingClientRect();
    var x = pageX - rect.left;
    var y = pageY - rect.top;
    return { x: x, y: y };
}
