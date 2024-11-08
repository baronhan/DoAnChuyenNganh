import joblib
import sys

model = joblib.load('D:/HKI 2024-2025/SmartRoomManagement/FinalProject/Scripts/model.pkl')

def predict(text):
    predicted_label = model.predict([text])[0]

    if predicted_label == 'good':
        return True
    else:
        return False

if len(sys.argv) < 2:
    print("Usage: python predict.py <text>")
    sys.exit(1)

text = sys.argv[1]

is_good = predict(text)
print(f"Prediction result: {'Good' if is_good else 'Bad'}")
