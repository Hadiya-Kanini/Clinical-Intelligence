#!/usr/bin/env python3
"""
Extract text from sample PDF to understand patient information format
"""
import PyPDF2
import sys

def extract_pdf_text(pdf_path):
    """Extract text from PDF file"""
    try:
        with open(pdf_path, 'rb') as file:
            pdf_reader = PyPDF2.PdfReader(file)
            
            print(f"PDF has {len(pdf_reader.pages)} page(s)")
            print("=" * 80)
            
            # Extract text from all pages
            full_text = ""
            for page_num, page in enumerate(pdf_reader.pages, 1):
                text = page.extract_text()
                print(f"\n--- Page {page_num} ---")
                print(text)
                print("-" * 80)
                full_text += text + "\n"
            
            # Save to file for analysis
            output_file = "sample_pdf_extracted_text.txt"
            with open(output_file, 'w', encoding='utf-8') as f:
                f.write(full_text)
            
            print(f"\n✅ Text extracted and saved to: {output_file}")
            return full_text
            
    except Exception as e:
        print(f"❌ Error extracting PDF: {e}")
        return None

if __name__ == "__main__":
    pdf_path = r"C:\Users\HadiyaAmber\Desktop\Clinical-Intelligence\Report_2 5.pdf"
    extract_pdf_text(pdf_path)
