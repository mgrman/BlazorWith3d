export function InitializeGlobalMouseEvents(elementTarget, dotnetObject, onMouseMoveMethodName, onPointerUpMethodName) {
    var onMoveCallback= function (o) {
       
        var res=ConvertPageToOffset(elementTarget, o.clientX, o.clientY);
        
        dotnetObject.invokeMethodAsync(onMouseMoveMethodName,res.x, res.y);
    } 
    var onUpCallback= function (o) {
        dotnetObject.invokeMethodAsync(onPointerUpMethodName)
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

export function InitializeGlobalTouchEvents(elementTarget, dotnetObject, onMouseMoveMethodName, onPointerUpMethodName) {
    var onMoveCallback= function (o) {

        var res=ConvertPageToOffset(elementTarget, o.touches[0].clientX, o.touches[0].clientY);

        dotnetObject.invokeMethodAsync(onMouseMoveMethodName,res.x, res.y);
    }
    var onUpCallback= function (o) {
        
        if(o.touches.length==0) {
        dotnetObject.invokeMethodAsync(onPointerUpMethodName)
        }
    }

    window.addEventListener("touchmove", onMoveCallback);
    window.addEventListener("touchend", onUpCallback);

    let disposeTracker = {}
    disposeTracker.Dispose=function (){

        window.removeEventListener("touchmove", onMoveCallback);
        window.removeEventListener("touchend", onUpCallback);
    }

    return disposeTracker;
}

export function ConvertPageToOffset(elementTarget, clientX, clientY) {

    var rect = elementTarget.getBoundingClientRect();
    var x = clientX - rect.left;
    var y = clientY - rect.top;
    return { x: x, y: y };
}

export function ConvertPageToOffsetIfInside(elementTarget, clientX, clientY) {

    var rect = elementTarget.getBoundingClientRect();
    var x = clientX - rect.left;
    var y = clientY - rect.top;
    
    if(x<0 || x> rect.width || y<0 || y > rect.height) {
        return null;
    }
    return { x: x, y: y };
}
