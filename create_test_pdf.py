from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import letter
import os

def create_test_pdf():
    """Create a simple test PDF for upload testing"""
    filename = "test-document.pdf"
    filepath = os.path.join(os.getcwd(), filename)
    
    # Create a simple PDF
    c = canvas.Canvas(filepath, pagesize=letter)
    width, height = letter
    
    # Add title
    c.setFont("Helvetica-Bold", 16)
    c.drawString(50, height - 50, "Test Document for Upload")
    
    # Add some content
    c.setFont("Helvetica", 12)
    text_content = [
        "This is a test document created for testing the upload functionality.",
        "It contains basic medical information for testing purposes.",
        "",
        "Patient Information:",
        "- Name: John Doe",
        "- Age: 45",
        "- Visit Date: 2025-01-16",
        "",
        "Symptoms:",
        "- Headache",
        "- Fatigue",
        "- Nausea",
        "",
        "Diagnosis:",
        "- Migraine",
        "- Stress-related symptoms",
        "",
        "Treatment:",
        "- Pain medication",
        "- Rest",
        "- Follow-up in 1 week"
    ]
    
    y_position = height - 100
    for line in text_content:
        c.drawString(50, y_position, line)
        y_position -= 20
    
    # Add a simple shape to make it more realistic
    c.rect(50, y_position - 20, 200, 60, stroke=1, fill=0)
    c.drawString(60, y_position - 35, "Medical Record Summary")
    
    c.save()
    print(f"Created test PDF: {filepath}")
    return filepath

if __name__ == "__main__":
    try:
        create_test_pdf()
    except ImportError:
        print("reportlab not installed. Installing...")
        import subprocess
        import sys
        subprocess.check_call([sys.executable, "-m", "pip", "install", "reportlab"])
        create_test_pdf()
