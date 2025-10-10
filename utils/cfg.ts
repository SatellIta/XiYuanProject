import * as fs from 'fs';
import * as path from 'path';
import lodash from 'lodash';

// 用于缓存已加载配置的缓存对象
let config: any = null;
let defaultConfig: any = null;

// 标志，用于确保文件只被尝试加载一次
let configLoaded = false;
let defaultConfigLoaded = false;

/**
 * 辅助函数：安全地加载一个 JSON 配置文件。
 * @param fileName 文件名（不带扩展名）
 * @returns 解析后的 JSON 对象，如果文件不存在或解析失败则返回 null。
 */
function loadConfigFile(fileName: string): any {
  const projectRoot = process.cwd();
  // 优先在 'config/' 目录中查找
  const filePath = path.join(projectRoot, 'config', `${fileName}.json`);

  if (fs.existsSync(filePath)) {
    try {
      const fileContent = fs.readFileSync(filePath, 'utf-8');
      return JSON.parse(fileContent);
    } catch (error) {
      console.error(`Error reading or parsing config file "${filePath}": ${error.message}`);
      return null;
    }
  }
  return null;
}

/**
 * 获取一个配置项的值。
 *
 * 它会首先在 `config/config.json` 文件中查找。如果找不到，
 * 则会回退到 `config/default_config.json` 文件中查找。
 *
 * @param key 要检索的配置键（例如 'port' 或 'database.host'）。
 * @param defaultKey 可选的默认键，如果在两个文件中都找不到 `key`，则尝试使用这个键查找。
 * @returns 配置项的值，如果在两个文件中都找不到，则返回 undefined。
 */
export function get(key: string, defaultKey?: any): any {
  // 首次调用时加载用户配置
  if (!configLoaded) {
    config = loadConfigFile('config');
    if (config !== null) {
      configLoaded = true;
    }
  }

  // 使用 lodash.get 尝试从用户配置中获取值
  let value = lodash.get(config, key);

  // 如果在用户配置中找到了值（即使是 null），则直接返回
  // 只有当值为 undefined (表示键不存在) 时，才继续查找默认配置
  if (value !== undefined) {
    return value;
  }

  // 如果用户配置中没有，则加载并从默认配置中查找
  if (!defaultConfigLoaded) {
    defaultConfig = loadConfigFile('default_config');
    defaultConfigLoaded = true;
  }

  // 尝试从默认cfg中获取值 
  value = lodash.get(defaultConfig, key);
  if (value !== undefined) {
    return value;
  }


  // 如果传入了默认值参数，则尝试从用户配置中获取它
  if (defaultKey !== undefined) {
    value = lodash.get(config, defaultKey);
  }

  if (value !== undefined) {
    return value;
  }

  // 如果传入了默认值参数，则尝试从默认配置中获取它
  if (defaultKey !== undefined) {
    value = lodash.get(defaultConfig, defaultKey);
  }

  if (value !== undefined) {
    return value;
  }

  // 如果都没有找到，则返回传入的默认值参数（可能是 undefined）
  if (defaultKey !== undefined) {
    return defaultKey;
  }

  return undefined;
}

/**
 * 设置一个配置项的值，并将其写入 `config/config.json`。
 *
 * 这会修改用户配置文件，如果文件不存在，则会创建它。
 *
 * @param key 要设置的配置键（例如 'port' 或 'database.host'）。
 * @param value 要设置的值。
 */
export function set(key: string, value: any): void {
  // 确保用户配置已加载或初始化为空对象
  if (!configLoaded) {
    config = loadConfigFile('config') || {};
    configLoaded = true;
  }

  // 使用 lodash.set 在内存中的配置对象上设置值
  lodash.set(config, key, value);

  // 将更新后的配置写回文件系统
  const projectRoot = process.cwd();
  const configDir = path.join(projectRoot, 'config');
  const filePath = path.join(configDir, 'config.json');

  try {
    // 确保 config 目录存在
    if (!fs.existsSync(configDir)) {
      fs.mkdirSync(configDir, { recursive: true });
    }
    // 将对象格式化为可读的 JSON 字符串并写入文件
    const fileContent = JSON.stringify(config, null, 2);
    fs.writeFileSync(filePath, fileContent, 'utf-8');
  } catch (error) {
    throw new Error(`Error writing config file "${filePath}": ${error.message}`);
  }
}

  /**
   * 删除一个配置项
   * @param key 要删除的键名
   */
  export function remove(key: string): void {
    if (!configLoaded) {
      config = loadConfigFile('config') || {};
      configLoaded = true;
    }

    // 使用 lodash.unset 删除指定键
    lodash.unset(config, key);

    // 将更新后的配置写回文件系统
    const projectRoot = process.cwd();
    const configDir = path.join(projectRoot, 'config');
    const filePath = path.join(configDir, 'config.json');
    
    try {
      // 确保 config 目录存在
      if (!fs.existsSync(configDir)) {
        fs.mkdirSync(configDir, { recursive: true });
      }
      // 将对象格式化为可读的 JSON 字符串并写入文件
      const fileContent = JSON.stringify(config, null, 2);
      fs.writeFileSync(filePath, fileContent, 'utf-8');
    } catch (error) {
      throw new Error(`Error writing config file "${filePath}": ${error.message}`);
    }
  }
