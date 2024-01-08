// @ts-check

// import * as prism from "prismjs";
// import "prismjs/components/prism-css";
// import "prismjs/components/prism-diff";
// import "prismjs/components/prism-javascript";
// import "prismjs/components/prism-json";
// import "prismjs/components/prism-jsx";
// import "prismjs/components/prism-markup";
// import "prismjs/components/prism-tsx";
// import "prismjs/components/prism-csharp";

import prism from "./npm-modified-prismjs";

function highlight(code, language) {
  let prismLanguage;
  switch (language) {
    case "cs":
      prismLanguage = prism.languages.csharp;
      break;
    case "ts":
      prismLanguage = prism.languages.tsx;
      break;

    case "js":
    case "sh":
      prismLanguage = prism.languages.jsx;
      break;

    case "diff":
      prismLanguage = { ...prism.languages.diff };
      // original `/^[-<].*$/m` matches lines starting with `<` which matches
      // <SomeComponent />
      // we will only use `-` as the deleted marker
      prismLanguage.deleted = /^[-].*$/m;
      break;

    default:
      prismLanguage = prism.languages[language];
      break;
  }

  if (!prismLanguage) {
    if (language) {
      throw new Error(`unsuppored language: "${language}", "${code}"`);
    } else {
      prismLanguage = prism.languages.jsx;
    }
  }

  return prism.highlight(code, prismLanguage, language);
}

function highlighter(element, code, language) {
  if (element) {
    const content = highlight(code, language);
    element.innerHTML = content;
  }
}

// @ts-ignore
window.Skclusive = {
  // @ts-ignore
  ...window.Skclusive,
  Script: {
    // @ts-ignore
    ...(window.Skclusive || {}).Script,
    Prism: {
      highlight,
      highlighter
    }
  }
};
