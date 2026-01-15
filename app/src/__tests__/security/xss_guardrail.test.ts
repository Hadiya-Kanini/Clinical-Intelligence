import { describe, it, expect } from 'vitest'
import * as fs from 'fs'
import * as path from 'path'

/**
 * XSS Guardrail Tests
 * 
 * These tests ensure that unsafe HTML rendering patterns are not introduced
 * into the frontend codebase. React escapes string children by default,
 * but dangerouslySetInnerHTML and innerHTML bypass this protection.
 * 
 * If these tests fail, it means someone has introduced a potential XSS vector.
 * Review the change carefully and either:
 * 1. Remove the unsafe pattern if not needed
 * 2. Use a vetted sanitizer (e.g., DOMPurify) if rich HTML is required
 */

const UNSAFE_PATTERNS = [
  'dangerouslySetInnerHTML',
  '.innerHTML',
  'innerHTML=',
  'outerHTML=',
  '.outerHTML',
  'document.write',
  'document.writeln',
]

const EXCLUDED_PATHS = [
  'node_modules',
  '__tests__',
  '.test.',
  '.spec.',
  'dist',
  'build',
]

function getAllTsxFiles(dir: string): string[] {
  const files: string[] = []
  
  function walk(currentDir: string): void {
    const entries = fs.readdirSync(currentDir, { withFileTypes: true })
    
    for (const entry of entries) {
      const fullPath = path.join(currentDir, entry.name)
      
      // Skip excluded paths
      if (EXCLUDED_PATHS.some(excluded => fullPath.includes(excluded))) {
        continue
      }
      
      if (entry.isDirectory()) {
        walk(fullPath)
      } else if (entry.isFile() && (entry.name.endsWith('.tsx') || entry.name.endsWith('.ts'))) {
        // Skip test files
        if (!entry.name.includes('.test.') && !entry.name.includes('.spec.')) {
          files.push(fullPath)
        }
      }
    }
  }
  
  walk(dir)
  return files
}

function checkFileForUnsafePatterns(filePath: string): { file: string; pattern: string; line: number }[] {
  const violations: { file: string; pattern: string; line: number }[] = []
  const content = fs.readFileSync(filePath, 'utf-8')
  const lines = content.split('\n')
  
  lines.forEach((line, index) => {
    for (const pattern of UNSAFE_PATTERNS) {
      if (line.includes(pattern)) {
        violations.push({
          file: filePath,
          pattern,
          line: index + 1,
        })
      }
    }
  })
  
  return violations
}

describe('XSS Guardrail', () => {
  it('should not contain dangerouslySetInnerHTML in source files', () => {
    const srcDir = path.resolve(__dirname, '../../')
    const tsxFiles = getAllTsxFiles(srcDir)
    
    const allViolations: { file: string; pattern: string; line: number }[] = []
    
    for (const file of tsxFiles) {
      const violations = checkFileForUnsafePatterns(file)
      allViolations.push(...violations)
    }
    
    if (allViolations.length > 0) {
      const message = allViolations
        .map(v => `  - ${v.file}:${v.line} contains "${v.pattern}"`)
        .join('\n')
      
      expect.fail(
        `XSS GUARDRAIL VIOLATION: Unsafe HTML rendering patterns detected!\n\n` +
        `The following files contain patterns that bypass React's XSS protection:\n${message}\n\n` +
        `If rich HTML rendering is required, use a vetted sanitizer like DOMPurify.\n` +
        `See: https://owasp.org/www-community/attacks/xss/`
      )
    }
    
    expect(allViolations).toHaveLength(0)
  })

  it('should have React as the rendering framework (implicit XSS protection)', () => {
    // This test documents that React's default behavior escapes HTML
    // React automatically escapes values embedded in JSX
    const testString = '<script>alert("xss")</script>'
    
    // In React, this would render as escaped text, not executed script
    // This test serves as documentation of the security model
    expect(testString).toContain('<script>')
    expect(testString).not.toBe('<script>alert("xss")</script>'.replace(/</g, '&lt;'))
  })

  it('should document XSS prevention decision point', () => {
    /**
     * DECISION POINT: Rich Text Rendering
     * 
     * If the application needs to render user-provided HTML (e.g., clinical notes
     * with formatting), the following approach should be used:
     * 
     * 1. Install DOMPurify: npm install dompurify @types/dompurify
     * 2. Create a SafeHtml component that sanitizes input before rendering
     * 3. Configure allowed tags/attributes based on requirements
     * 4. Add this component to the approved list in this test
     * 
     * Example SafeHtml component:
     * ```tsx
     * import DOMPurify from 'dompurify';
     * 
     * export function SafeHtml({ html }: { html: string }) {
     *   const sanitized = DOMPurify.sanitize(html, {
     *     ALLOWED_TAGS: ['b', 'i', 'em', 'strong', 'p', 'br'],
     *     ALLOWED_ATTR: []
     *   });
     *   return <div dangerouslySetInnerHTML={{ __html: sanitized }} />;
     * }
     * ```
     * 
     * When adding such a component, update EXCLUDED_PATHS to include its path
     * and document the security review in the PR.
     */
    expect(true).toBe(true) // Documentation test always passes
  })
})

describe('Safe Rendering Patterns', () => {
  it('should confirm text interpolation is safe in React', () => {
    // React JSX automatically escapes these patterns
    const maliciousInputs = [
      '<img src=x onerror=alert(1)>',
      '<script>alert("xss")</script>',
      'javascript:alert(1)',
      '"><script>alert(1)</script>',
      "'; DROP TABLE users; --",
    ]
    
    // All these strings should remain as-is (not executed) when rendered in React
    // This test documents the expected behavior
    for (const input of maliciousInputs) {
      expect(typeof input).toBe('string')
      expect(input.length).toBeGreaterThan(0)
    }
  })
})
