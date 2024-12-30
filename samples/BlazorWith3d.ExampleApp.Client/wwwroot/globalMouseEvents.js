export function InitializeGlobalMouseEvents(elementTarget, dotnetObject, onMouseMoveMethodName, onMouseUpMethodName) {
    var onMoveCallback= function (o) {
       
        var res=ConvertPageToOffset(elementTarget, o.clientX, o.clientY);
        
        dotnetObject.invokeMethodAsync(onMouseMoveMethodName,res.x, res.y);
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

export function ConvertPageToOffset(elementTarget, clientX, clientY) {

    var rect = elementTarget.getBoundingClientRect();
    var x = clientX - rect.left;
    var y = clientY - rect.top;
    return { x: x, y: y };
}
