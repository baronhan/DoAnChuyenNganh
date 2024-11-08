import sys
from ultralytics import YOLO

def detect_objects(image_path):
    model = YOLO('yolov8n.pt')
    results = model(image_path)
    
    unsuitable_objects = ['person', 'car', 'dog', 'cat', 'horse', 'sheep', 'cow', 'bird', 'truck', 'bus', 'motorcycle', 'bicycle', 'airplane', 'train', 'van', 'boat']
    
    detected_objects = []

    for result in results:
        for box in result.boxes:
            print(f"Detected label: {box.cls}, confidence: {box.conf}")
            label = box.cls  
            label_name = model.names[int(label)] 
            print(f"Label name: {label_name}")
            if label_name in unsuitable_objects:
                detected_objects.append(label_name)

    if detected_objects:
        return False, detected_objects  
    return True, [] 

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python yolo_detection.py <image_path>")
        sys.exit(1)

    image_path = sys.argv[1]
    is_valid, detected_objects = detect_objects(image_path)
    
    if is_valid:
        print("Image is valid for use.")
    else:
        print(f"Image contains unsuitable objects: {', '.join(detected_objects)}")
