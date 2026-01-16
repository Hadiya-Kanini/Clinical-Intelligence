---
trigger: glob
globs: "**/*.md"
---

# Markdown Content Rules

## Content Rules
1. **Headings:** Use appropriate levels (H2, H3); do not use H1 (generated from title)
2. **Lists:** Use bullet points or numbered lists with proper indentation and spacing
3. **Code Blocks:** Use fenced code blocks; specify language for syntax highlighting
4. **Links:** Use proper markdown syntax; ensure links are valid and accessible
5. **Images:** Use proper markdown syntax; include alt text for accessibility
6. **Tables:** Use markdown tables for tabular data; ensure proper formatting and alignment
7. **Line Length:** Limit to 400 characters for readability
8. **Whitespace:** Use appropriate whitespace to separate sections
9. **Front Matter:** Include YAML front matter at beginning with required metadata fields

## Formatting and Structure

- **Headings:** Use `##` for H2 and `###` for H3 hierarchically; recommend restructuring if H4+ needed
- **Lists:** Use `-` for bullets and `1.` for numbered; indent nested lists with two spaces
- **Code Blocks:** Use triple backticks; specify language after opening backticks (e.g., ` ```csharp`)
- **Links:** Use `[link text](URL)` with descriptive text and valid URL
- **Images:** Use `![alt text](image URL)` with brief description in alt text
- **Tables:** Use `|` to create tables with proper alignment and headers
- **Line Length:** Break lines at 80 characters for readability; use soft line breaks for long paragraphs
- **Whitespace:** Use blank lines to separate sections; avoid excessive whitespace

## Validation Requirements

### Front Matter Fields
- `post_title`: Title of the post
- `author1`: Primary author of the post
- `post_slug`: URL slug for the post
- `microsoft_alias`: Microsoft alias of the author
- `featured_image`: URL of the featured image
- `categories`: Categories for the post (must be from list in /categories.txt)
- `tags`: Tags for the post
- `ai_note`: Indicate if AI was used in creation
- `summary`: Brief summary of the post (recommend based on content when possible)
- `post_date`: Publication date of the post

### Compliance
- Ensure content follows markdown content rules specified above
- Ensure content is properly formatted and structured per guidelines
- Run validation tools to check for compliance with rules and guidelines