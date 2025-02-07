import React, { useCallback, useEffect, useRef } from "react"

function DraggableFix({ children, style, className, onDragStart, onDragEnd }) {
    const dragRef = useRef(null)

    const onContextMenu = () => {
    }

    useEffect(() => {
        const dragDiv = dragRef.current

        return () => {

            document.removeEventListener("contextmenu", onContextMenu, false)
        }
    }, [])

    return (
        <div
            ref={dragRef}
            className={className || "drag-react"}
            style={{
                position: "fixed",
                left: "10px",
                top: "10px",
                cursor: "move",
                ...style
            }}
        >
            {children}
        </div>
    )
}

export { DraggableFix }
